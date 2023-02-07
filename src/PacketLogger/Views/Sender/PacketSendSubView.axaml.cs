//
//  PacketSendSubView.axaml.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace PacketLogger.Views.Sender;

[DoNotNotify]
public partial class PacketSendSubView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PacketSendSubView"/> class.
    /// </summary>
    public PacketSendSubView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}