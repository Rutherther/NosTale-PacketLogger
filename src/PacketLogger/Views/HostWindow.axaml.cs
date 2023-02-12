//
//  HostWindow.axaml.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace PacketLogger.Views;

/// <summary>
/// A floating window styled same as main window.
/// </summary>
[DoNotNotify]
public partial class HostWindow : Dock.Avalonia.Controls.HostWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HostWindow"/> class.
    /// </summary>
    public HostWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}