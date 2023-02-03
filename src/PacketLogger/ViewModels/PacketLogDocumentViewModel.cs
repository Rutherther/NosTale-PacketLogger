//
//  PacketLogDocumentViewModel.cs
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

namespace PacketLogger.ViewModels;

/// <inheritdoc />
public class PacketLogDocumentViewModel : Document, INotifyPropertyChanged, IDisposable
{
    private readonly CommsInjector _injector;
    private readonly NostaleProcesses _processes;
    private CancellationTokenSource _ctSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketLogDocumentViewModel"/> class.
    /// </summary>
    /// <param name="injector">The injector.</param>
    /// <param name="repository">The repository.</param>
    /// <param name="processes">The NosTale processes collection.</param>
    public PacketLogDocumentViewModel(CommsInjector injector, StatefulRepository repository, NostaleProcesses processes)
    {
        _ctSource = new CancellationTokenSource();
        _injector = injector;
        _processes = processes;
        OpenDummy = ReactiveCommand.CreateFromTask
        (
            () => Task.Run
            (
                () =>
                {
                    Loading = true;
                    Name = "Dummy";
                    LogViewModel = new LogTabViewModel(new DummyPacketProvider());
                    Loaded = true;
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

                var openResult = await provider.Open();
                if (!openResult.IsSuccess)
                {
                    Console.WriteLine("Could not open the file.");
                    return;
                }

                Title = Path.GetFileName(path);
                LogViewModel = new LogTabViewModel(provider);
                Loaded = true;
                Loading = false;
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

                var provider = new CommsPacketProvider(connection);
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

                process.WhenPropertyChanged(x => x.CharacterString)
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

                LogViewModel = new LogTabViewModel(provider);
                Title = handshakeResponse.CharacterName ?? $"Not in game ({process.Process.Id})";
                Loading = false;
                Loaded = true;
            }
        );
    }

    /// <summary>
    /// Gets the processes observable.
    /// </summary>
    public ObservableCollection<NostaleProcess> Processes => _processes.Processes;

    /// <summary>
    /// Gets or sets the name of the tab.
    /// </summary>
    public string Name { get; set; } = "New tab";

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
    public LogTabViewModel? LogViewModel { get; private set; }

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
        LogViewModel?.Provider.Close().GetAwaiter().GetResult();
        Dispose();
        return base.OnClose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _ctSource.Cancel();
        _ctSource.Dispose();
        LogViewModel?.Dispose();
        OpenDummy.Dispose();
        OpenFile.Dispose();
        OpenProcess.Dispose();
    }
}