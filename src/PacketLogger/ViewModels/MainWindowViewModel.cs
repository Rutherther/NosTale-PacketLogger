//
//  MainWindowViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
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
        _factory = new DockFactory(this);

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
