//
//  PacketLogDocumentView.axaml.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace PacketLogger.Views;

[DoNotNotify]
public partial class PacketLogDocumentView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PacketLogDocumentView"/> class.
    /// </summary>
    public PacketLogDocumentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}