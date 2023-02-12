//
//  PacketSenderView.axaml.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace PacketLogger.Views.Sender;

/// <summary>
/// A view used for sending and receiving packets.
/// </summary>
[DoNotNotify]
public partial class PacketSenderView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PacketSenderView"/> class.
    /// </summary>
    public PacketSenderView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}