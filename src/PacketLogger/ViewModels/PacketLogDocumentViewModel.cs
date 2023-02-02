//
//  PacketLogDocumentViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Model.Mvvm.Controls;
using PacketLogger.Models;
using PacketLogger.Models.Packets;
using ReactiveUI;

namespace PacketLogger.ViewModels;

/// <inheritdoc />
public class PacketLogDocumentViewModel : Document, INotifyPropertyChanged
{
    private readonly NostaleProcesses _processes;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketLogDocumentViewModel"/> class.
    /// </summary>
    /// <param name="processes">The NosTale processes collection.</param>
    public PacketLogDocumentViewModel(NostaleProcesses processes)
    {
        _processes = processes;
        OpenDummy = ReactiveCommand.CreateFromTask
        (
            () => Task.Run(() =>
            {
                Loading = true;
                Name = "Dummy";
                LogViewModel = new LogTabViewModel(new DummyPacketProvider());
                Loaded = true;
            })
        );
        OpenFile = ReactiveCommand.CreateFromTask
        (
            async () =>
            {
                var mainWindow = (App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                    ?.MainWindow;
                var result = await new OpenFileDialog()
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

                LogViewModel = new LogTabViewModel(provider);
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
    /// Gets or sets the name of the tab.
    /// </summary>
    public string Name { get; set; } = "New tab";

    /// <summary>
    /// Gets whether the document is currently being loaded.
    /// </summary>
    public bool Loading { get; private set; } = false;

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
}