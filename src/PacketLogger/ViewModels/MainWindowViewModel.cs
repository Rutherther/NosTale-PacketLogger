//
//  MainWindowViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dock.Model.Controls;
using Dock.Model.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NosSmooth.Comms.Local.Extensions;
using NosSmooth.Core.Extensions;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using NosSmooth.PacketSerializer.Extensions;
using NosSmooth.PacketSerializer.Packets;
using PacketLogger.Models;
using PacketLogger.Models.Packets;
using PacketLogger.Views;
using ReactiveUI;

namespace PacketLogger.ViewModels;

/// <inheritdoc />
public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly DockFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    public MainWindowViewModel()
    {
        var services = new ServiceCollection()
            .AddLogging(b => b.ClearProviders().AddConsole())
            .AddSingleton<DockFactory>()
            .AddNostaleCore()
            .AddStatefulInjector()
            .AddStatefulEntity<CommsPacketProvider>()
            .AddLocalComms()
            .AddPacketResponder<PacketResponder>()
            .BuildServiceProvider();

        var packetTypes = services.GetRequiredService<IPacketTypesRepository>();

        var result = packetTypes.AddDefaultPackets();
        if (!result.IsSuccess)
        {
            Console.WriteLine(result.ToFullString());
        }

        _factory = services.GetRequiredService<DockFactory>();
        Layout = _factory.CreateLayout();
        if (Layout is { })
        {
            _factory.InitLayout(Layout);
            if (Layout is { } root)
            {
                root.Navigate.Execute("Home");
            }
        }

        SaveAll = ReactiveCommand.CreateFromTask
        (
            async () =>
            {
                if (Layout?.FocusedDockable is PacketLogDocumentViewModel activeDocument && activeDocument.Loaded
                    && activeDocument.LogViewModel is not null)
                {
                    var mainWindow = (App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                        ?.MainWindow;
                    var result = await new SaveFileDialog()
                    {
                        DefaultExtension = ".log",
                        InitialFileName = Assembly.GetEntryAssembly()?.GetModules().FirstOrDefault()?.FullyQualifiedName
                    }.ShowAsync(mainWindow!);

                    if (result is null)
                    {
                        return;
                    }

                    using var file = File.OpenWrite(result);
                    using var streamWriter = new StreamWriter(file);
                    foreach (var packet in activeDocument.LogViewModel.Provider.Packets.Items)
                    {
                        await streamWriter.WriteLineAsync
                        (
                            $"[{packet.Date:HH:mm:ss}]\t[{(packet.Source == PacketSource.Server ? "Recv" : "Send")}]\t{packet.PacketString.Trim()}"
                        );
                    }
                }
            }
        );

        SaveFiltered = ReactiveCommand.CreateFromTask
        (
            async () =>
            {
                if (Layout?.FocusedDockable is PacketLogDocumentViewModel activeDocument && activeDocument.Loaded
                    && activeDocument.LogViewModel is not null)
                {
                    var mainWindow = (App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                        ?.MainWindow;
                    var result = await new SaveFileDialog()
                    {
                        DefaultExtension = ".log",
                    }.ShowAsync(mainWindow!);

                    if (result is null)
                    {
                        return;
                    }

                    using var file = File.OpenWrite(result);
                    using var streamWriter = new StreamWriter(file);
                    foreach (var packet in activeDocument.LogViewModel.FilteredPackets)
                    {
                        await streamWriter.WriteLineAsync
                        (
                            $"[{packet.Date:HH:mm:ss}]\t[{(packet.Source == PacketSource.Server ? "Recv" : "Send")}]\t{packet.PacketString.Trim()}"
                        );
                    }
                }
            }
        );

        OpenFile = ReactiveCommand.Create
            (() => _factory.CreateLoadedDocument(doc => doc.OpenFile.Execute(Unit.Default)));

        OpenEmpty = ReactiveCommand.Create
            (() => _factory.CreateLoadedDocument(doc => doc.OpenDummy.Execute(Unit.Default)));

        NewTab = ReactiveCommand.Create
            (() => _factory.DocumentDock.CreateDocument?.Execute(null));

        QuitApplication = ReactiveCommand.Create
            (() => (Application.Current?.ApplicationLifetime as IControlledApplicationLifetime)?.Shutdown());
    }

    /// <summary>
    /// Gets or sets the layout.
    /// </summary>
    public IRootDock? Layout { get; set; }

    /// <summary>
    /// Gets a command that quits the application.
    /// </summary>
    public ReactiveCommand<Unit, Unit> QuitApplication { get; }

    /// <summary>
    /// Gets a command that saves all packets.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveAll { get; }

    /// <summary>
    /// Gets a command that saves filtered packets.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveFiltered { get; }

    /// <summary>
    /// Gets the comamnd that opens a file.
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenFile { get; }

    /// <summary>
    /// Gets the command that opens empty logger.
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenEmpty { get; }

    /// <summary>
    /// Gets the command that opens a new tab.
    /// </summary>
    public ReactiveCommand<Unit, Unit> NewTab { get; }
}