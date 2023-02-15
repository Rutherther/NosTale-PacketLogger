//
//  DocumentViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Model.Mvvm.Controls;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using NosSmooth.Comms.Data.Messages;
using NosSmooth.Comms.Inject.Messages;
using NosSmooth.Comms.Local;
using NosSmooth.Comms.Local.Extensions;
using NosSmooth.Core.Contracts;
using NosSmooth.Core.Extensions;
using NosSmooth.Core.Stateful;
using NosSmooth.Pcap;
using PacketLogger.Models;
using PacketLogger.Models.Filters;
using PacketLogger.Models.Packets;
using PacketLogger.Models.Titles;
using PacketLogger.ViewModels.Log;
using PacketLogger.ViewModels.Sender;
using PacketLogger.ViewModels.Settings;
using ReactiveUI;
using Remora.Results;

namespace PacketLogger.ViewModels;

/// <inheritdoc />
public class DocumentViewModel : Document, INotifyPropertyChanged, IDisposable
{
    private readonly ObservableCollection<IPacketProvider> _providers;
    private readonly NostaleProcesses _processes;
    private readonly Action<DocumentViewModel> _onDocumentUnloaded;
    private CancellationTokenSource _ctSource;
    private IPacketProvider? _packetProvider;
    private IDisposable? _cleanUp;
    private NumberedTitleGenerator.TitleHandle _titleHandle;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentViewModel"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="filterProfiles">The filter profiles.</param>
    /// <param name="injector">The injector.</param>
    /// <param name="repository">The repository.</param>
    /// <param name="providers">The providers.</param>
    /// <param name="processes">The NosTale processes collection.</param>
    /// <param name="titleGenerator">The title generator.</param>
    /// <param name="onDocumentLoaded">The action to call on loaded.</param>
    /// <param name="onDocumentUnloaded">The action to call on document unloaded/closed.</param>
    public DocumentViewModel
    (
        IServiceProvider services,
        FilterProfiles filterProfiles,
        CommsInjector injector,
        StatefulRepository repository,
        ObservableCollection<IPacketProvider> providers,
        NostaleProcesses processes,
        NumberedTitleGenerator titleGenerator,
        Action<DocumentViewModel> onDocumentLoaded,
        Action<DocumentViewModel> onDocumentUnloaded
    )
    {
        _titleHandle = titleGenerator.AddTitle
        (
            title => Title = title,
            Observable.Empty<string>(),
            "New tab"
        );

        _cleanUp = this.WhenAnyValue(x => x.Title)
            .Subscribe(title =>
                {
                    if (_packetProvider is not null)
                    {
                        _packetProvider.DocumentTitle = title;
                    }
                }
            );

        FilterProfiles = filterProfiles;
        _ctSource = new CancellationTokenSource();
        _providers = providers;
        _processes = processes;
        _onDocumentUnloaded = onDocumentUnloaded;
        OpenDummy = ReactiveCommand.CreateFromTask
        (
            () => Task.Run
            (
                () =>
                {
                    Loading = true;
                    _packetProvider = new DummyPacketProvider(Title);
                    _packetProvider.DocumentTitle = Title;
                    NestedViewModel = new PacketLogViewModel(_packetProvider, filterProfiles);
                    Loaded = true;
                    onDocumentLoaded(this);
                }
            )
        );

        OpenFile = ReactiveCommand.CreateFromTask
        (
            async () =>
            {
                var mainWindow = (App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                    ?.MainWindow;
                var result = await new OpenFileDialog
                {
                    AllowMultiple = false,
                    InitialFileName = Assembly.GetEntryAssembly()?.GetModules().FirstOrDefault()?.FullyQualifiedName
                }.ShowAsync(mainWindow!);

                if (result is null || result.Length == 0)
                {
                    return;
                }
                Loading = true;

                var path = result[0];
                var provider = new FilePacketProvider(path);
                _packetProvider = provider;

                var openResult = await provider.Open();
                if (!openResult.IsSuccess)
                {
                    Error = "File could not be opened. " + openResult.ToFullString();
                    Loading = false;
                    return;
                }

                _titleHandle?.Dispose();
                _titleHandle = titleGenerator.AddTitle
                (
                    title => Title = title,
                    Observable.Empty<string>(),
                    Path.GetFileName(path)
                );
                _packetProvider.DocumentTitle = Title;
                NestedViewModel = new PacketLogViewModel(provider, filterProfiles);
                Loaded = true;
                Loading = false;
                onDocumentLoaded(this);
            }
        );

        OpenProcess = ReactiveCommand.CreateFromTask<NostaleProcess>
        (
            async (process, ct) =>
            {
                Loading = true;
                var connectionResult = await injector.EstablishNamedPipesConnectionAsync
                    (process.Process, _ctSource.Token, ct);
                if (!connectionResult.IsDefined(out var connection))
                {
                    Error = "An error has occurred upon establishing a connection: " + connectionResult.ToFullString();
                    Loading = false;
                    return;
                }

                var provider = new CommsPacketProvider(process, connection);
                _packetProvider = provider;
                repository.SetEntity<ClientPacketProvider>(connection.Client, provider);

                var contractResult = await connection.Connection.ContractHanshake
                        (new HandshakeRequest("PacketLogger", true, false))
                    .WaitForAsync(DefaultStates.ResponseObtained, ct: ct);

                if (!contractResult.IsDefined(out var handshakeResponse))
                {
                    repository.Remove(connection.Client);
                    Error = "An error has occurred upon sending handshake: " + contractResult.ToFullString();
                    Loading = false;
                    return;
                }

                var handshakeInitResponse = handshakeResponse.InitializationResult;
                if (handshakeInitResponse is not null && !handshakeInitResponse.Value.IsSuccess)
                {
                    repository.Remove(connection.Client);
                    Error = "An error has occurred during handshaking: " + handshakeInitResponse.ToFullString();
                    Loading = false;
                    return;
                }

                if (handshakeInitResponse is null)
                {
                    var runClientContractResult = await connection.Connection.ContractRunClient(new RunClientRequest())
                        .WaitForAsync(DefaultStates.ResponseObtained, ct: ct);

                    if (!runClientContractResult.IsDefined(out var runClientResponse))
                    {
                        repository.Remove(connection.Client);
                        Error = "An error has occurred upon sending run client: " + runClientContractResult.ToFullString();
                        Loading = false;
                        return;
                    }

                    if (runClientResponse.InitializationResult is null)
                    {
                        repository.Remove(connection.Client);
                        Error = "Unknown error";
                        Loading = false;
                        return;
                    }

                    if (runClientResponse.BindingManagerResult is not null && !runClientResponse.BindingManagerResult.Value.IsSuccess)
                    {
                        Console.WriteLine("There was an error in binding initialization.");
                        Console.WriteLine(runClientResponse.BindingManagerResult.Value.ToFullString());
                    }

                    if (!runClientResponse.InitializationResult.Value.IsSuccess)
                    {
                        repository.Remove(connection.Client);
                        Error = "An error has occurred during starting client: " + runClientResponse.InitializationResult.ToFullString();
                        Loading = false;
                        return;
                    }
                }

                _titleHandle?.Dispose();
                _titleHandle = titleGenerator.AddTitle
                (
                    title => Title = title,
                    provider.WhenAnyValue(x => x.Name).ObserveOn(RxApp.MainThreadScheduler),
                    provider.Name
                );

                _packetProvider.DocumentTitle = Title;
                NestedViewModel = new PacketLogViewModel(provider, filterProfiles);
                Loading = false;
                Loaded = true;
                onDocumentLoaded(this);
            }
        );

        OpenPcap = ReactiveCommand.CreateFromTask<NostaleProcess>
        (
            async process =>
            {
                Loading = true;

                var initialEncryptionKey = 0;
                if (process.BrowserManager.IsInGame.Get())
                {
                    var encryptionKeyOptional = process.BrowserManager.NtClient.Map(client => client.EncryptionKey);
                    encryptionKeyOptional.TryGet(out initialEncryptionKey);
                }

                var client = ActivatorUtilities.CreateInstance<PcapNostaleClient>
                    (services, process.Process, initialEncryptionKey, Encoding.Default);

                var provider = new PcapPacketProvider(process, client);
                _packetProvider = provider;
                repository.SetEntity<ClientPacketProvider>(client, provider);

                _titleHandle?.Dispose();
                _titleHandle = titleGenerator.AddTitle
                (
                    title => Title = title,
                    provider
                        .WhenAnyValue(x => x.Name)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Select(x => x + " - sniff"),
                    provider.Name
                );
                await provider.Open();

                _packetProvider.DocumentTitle = Title;
                NestedViewModel = new PacketLogViewModel(provider, filterProfiles);
                Loading = false;
                Loaded = true;
                onDocumentLoaded(this);
            }
        );

        OpenSender = ReactiveCommand.Create<IPacketProvider>
        (
            provider =>
            {
                Loading = true;
                NestedViewModel = new PacketSenderViewModel(provider);
                _titleHandle?.Dispose();
                _titleHandle = titleGenerator.AddTitle
                (
                    title => Title = title,
                    provider
                        .WhenAnyValue(x => x.DocumentTitle)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Select(x => $"Sender - {provider.DocumentTitle}"),
                    provider.Name
                );
                Loaded = true;
                Loading = false;
            }
        );

        ClearError = ReactiveCommand.Create(() => Error = null);

        OpenSettings = ReactiveCommand.Create
        (
            () =>
            {
                _titleHandle?.Dispose();
                _titleHandle = titleGenerator.AddTitle
                (
                    title => Title = title,
                    Observable.Empty<string>(),
                    "Settings"
                );
                NestedViewModel = new SettingsViewModel(filterProfiles);
                Loaded = true;
            }
        );
    }

    /// <summary>
    /// Gets the filter profiles.
    /// </summary>
    public FilterProfiles FilterProfiles { get; }

    /// <summary>
    /// Gets the processes observable.
    /// </summary>
    public NostaleProcesses Processes => _processes;

    /// <summary>
    /// Gets packet provider.
    /// </summary>
    public IPacketProvider? Provider => _packetProvider;

    /// <summary>
    /// Gets the open providers.
    /// </summary>
    public ObservableCollection<IPacketProvider> Providers => _providers;

    /// <summary>
    /// Gets whether the document is currently being loaded.
    /// </summary>
    public bool Loading { get; private set; }

    /// <summary>
    /// Gets whether a document has been loaded.
    /// </summary>
    public bool Loaded { get; private set; }

    /// <summary>
    /// Gets or sets the current error.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Gets or sets whether there is an error.
    /// </summary>
    public bool HasError => Error is not null;

    /// <summary>
    /// Gets the log tab view model.
    /// </summary>
    public ViewModelBase? NestedViewModel { get; private set; }

    /// <summary>
    /// Gets command for opening a dummy.
    /// </summary>
    public ReactiveCommand<IPacketProvider, Unit> OpenSender { get; }

    /// <summary>
    /// Gets command for opening a dummy.
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenDummy { get; }

    /// <summary>
    /// Gets command for opening a file.
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenFile { get; }

    /// <summary>
    /// Gets the command to clear the error.
    /// </summary>
    public ReactiveCommand<Unit, string?> ClearError { get; }

    /// <summary>
    /// Gets the command for opening a process / connecting to a process.
    /// </summary>
    public ReactiveCommand<NostaleProcess, Unit> OpenProcess { get; }

    /// <summary>
    /// Gets the command for opening a process / connecting to a process.
    /// </summary>
    public ReactiveCommand<NostaleProcess, Unit> OpenPcap { get; }

    /// <summary>
    /// Get open settings command.
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenSettings { get; }

    /// <inheritdoc />
    public override bool OnClose()
    {
        _onDocumentUnloaded(this);
        var result = _packetProvider?.Close().GetAwaiter().GetResult();
        Dispose();

        if (!(result?.IsSuccess ?? true))
        {
            Console.WriteLine(result.ToFullString());
        }
        return base.OnClose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _titleHandle?.Dispose();
        _cleanUp?.Dispose();
        _ctSource.Cancel();
        _ctSource.Dispose();
        (NestedViewModel as IDisposable)?.Dispose();
        OpenDummy.Dispose();
        OpenFile.Dispose();
        OpenProcess.Dispose();
        OpenSender.Dispose();
    }
}