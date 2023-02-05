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
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Model.Mvvm.Controls;
using DynamicData.Binding;
using NosSmooth.Comms.Data.Messages;
using NosSmooth.Comms.Local;
using NosSmooth.Core.Contracts;
using NosSmooth.Core.Extensions;
using NosSmooth.Core.Stateful;
using PacketLogger.Models;
using PacketLogger.Models.Packets;
using ReactiveUI;
using Remora.Results;

namespace PacketLogger.ViewModels;

/// <inheritdoc />
public class DocumentViewModel : Document, INotifyPropertyChanged, IDisposable
{
    private readonly CommsInjector _injector;
    private readonly ObservableCollection<IPacketProvider> _providers;
    private readonly NostaleProcesses _processes;
    private readonly Action<DocumentViewModel> _onDocumentUnloaded;
    private CancellationTokenSource _ctSource;
    private IPacketProvider? _packetProvider;
    private IDisposable? _cleanUp;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentViewModel"/> class.
    /// </summary>
    /// <param name="injector">The injector.</param>
    /// <param name="repository">The repository.</param>
    /// <param name="providers">The providers.</param>
    /// <param name="processes">The NosTale processes collection.</param>
    /// <param name="onDocumentLoaded">The action to call on loaded.</param>
    /// <param name="onDocumentUnloaded">The action to call on document unloaded/closed.</param>
    public DocumentViewModel
    (
        CommsInjector injector,
        StatefulRepository repository,
        ObservableCollection<IPacketProvider> providers,
        NostaleProcesses processes,
        Action<DocumentViewModel> onDocumentLoaded,
        Action<DocumentViewModel> onDocumentUnloaded
    )
    {
        _ctSource = new CancellationTokenSource();
        _injector = injector;
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
                    NestedViewModel = new PacketLogViewModel(_packetProvider);
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
                    Console.WriteLine("Could not open the file.");
                    return;
                }

                Title = Path.GetFileName(path);
                NestedViewModel = new PacketLogViewModel(provider);
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
                    Console.WriteLine(connectionResult.ToFullString());
                    return;
                }

                var provider = new CommsPacketProvider(process, connection);
                _packetProvider = provider;
                repository.SetEntity<CommsPacketProvider>(connection.Client, provider);

                var contractResult = await connection.Connection.ContractHanshake
                        (new HandshakeRequest("PacketLogger", true, false))
                    .WaitForAsync(DefaultStates.ResponseObtained, ct: ct);

                if (!contractResult.IsDefined(out var handshakeResponse))
                {
                    repository.Remove(connection.Client);
                    Console.WriteLine(contractResult.ToFullString());
                    return;
                }

                var handshakeInitResponse = handshakeResponse.InitializationErrorfulResult ?? Result.FromSuccess();
                if (!handshakeInitResponse.IsSuccess)
                {
                    repository.Remove(connection.Client);
                    Console.WriteLine(handshakeInitResponse.ToFullString());
                    return;
                }

                _cleanUp = process.WhenPropertyChanged(x => x.CharacterString)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Do
                    (
                        _ =>
                        {
                            Title = (process.BrowserManager.IsInGame
                                ? process.BrowserManager.PlayerManager.Player.Name
                                : null) ?? $"Not in game ({process.Process.Id})";
                        }
                    )
                    .Subscribe();

                NestedViewModel = new PacketLogViewModel(provider);
                Title = handshakeResponse.CharacterName ?? $"Not in game ({process.Process.Id})";
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
                    Title = $"Sender ({provider.Name})";
                    Loaded = true;
                    Loading = false;
                }
            );
    }

    /// <summary>
    /// Gets the processes observable.
    /// </summary>
    public ObservableCollection<NostaleProcess> Processes => _processes.Processes;

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
    /// Gets the command for opening a process / connecting to a process.
    /// </summary>
    public ReactiveCommand<NostaleProcess, Unit> OpenProcess { get; }

    /// <inheritdoc />
    public override bool OnClose()
    {
        _onDocumentUnloaded(this);
        _packetProvider?.Close().GetAwaiter().GetResult();
        Dispose();
        return base.OnClose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
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