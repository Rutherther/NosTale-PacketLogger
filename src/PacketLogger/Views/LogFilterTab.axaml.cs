//
//  LogFilterTab.axaml.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PacketLogger.Models.Filters;
using PropertyChanged;

namespace PacketLogger.Views;

[DoNotNotify]
public partial class LogFilterTab : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogFilterTab"/> class.
    /// </summary>
    public LogFilterTab()
    {
        InitializeComponent();
        this.FindControl<ComboBox>("FilterType").Items = Enum.GetValues<FilterCreator.FilterType>();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}