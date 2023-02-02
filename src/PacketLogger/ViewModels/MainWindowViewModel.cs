//
//  MainWindowViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NosSmooth.Comms.Local.Extensions;
using NosSmooth.Core.Extensions;
using NosSmooth.PacketSerializer.Extensions;
using NosSmooth.PacketSerializer.Packets;
using PacketLogger.Models;
using PacketLogger.Models.Packets;

namespace PacketLogger.ViewModels;

/// <inheritdoc />
public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly IFactory? _factory;

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

        Layout = _factory?.CreateLayout();
        if (Layout is { })
        {
            _factory?.InitLayout(Layout);
            if (Layout is { } root)
            {
                root.Navigate.Execute("Home");
            }
        }
    }

    /// <summary>
    /// Gets or sets the layout.
    /// </summary>
    public IRootDock? Layout { get; set; }
}
