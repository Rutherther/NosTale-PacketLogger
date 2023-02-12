//
//  FilterEntryView.axaml.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PacketLogger.Models.Filters;
using PropertyChanged;

namespace PacketLogger.Views.Filters;

/// <summary>
/// A view for modifying filter entry.
/// </summary>
[DoNotNotify]
public partial class FilterEntryView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterEntryView"/> class.
    /// </summary>
    public FilterEntryView()
    {
        InitializeComponent();
        this.FindControl<ComboBox>("FilterType").Items = Enum.GetValues<FilterCreator.FilterType>();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}