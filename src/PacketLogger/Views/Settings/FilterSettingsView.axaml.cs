//
//  FilterSettingsView.axaml.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace PacketLogger.Views.Settings;

[DoNotNotify]
public partial class FilterSettingsView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterSettingsView"/> class.
    /// </summary>
    public FilterSettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}