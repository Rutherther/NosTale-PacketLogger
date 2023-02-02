//
//  LogTabView.axaml.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace PacketLogger.Views;

[DoNotNotify]
public partial class LogTabView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogTabView"/> class.
    /// </summary>
    public LogTabView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void PacketsLog_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem is not null)
        {
            dataGrid.ScrollIntoView(dataGrid.SelectedItem, dataGrid.Columns.First());
        }
    }
}