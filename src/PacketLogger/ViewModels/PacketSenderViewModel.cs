//
//  PacketSenderViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reactive;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using PacketLogger.Models.Packets;
using ReactiveUI;

namespace PacketLogger.ViewModels;

/// <inheritdoc />
public class PacketSenderViewModel : ViewModelBase, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PacketSenderViewModel"/> class.
    /// </summary>
    /// <param name="packetSender">The packet sender.</param>
    public PacketSenderViewModel(IPacketProvider packetSender)
    {
        RecvSubViewModel = new PacketSendSubViewModel(PacketSource.Server, packetSender);
        SendSubViewModel = new PacketSendSubViewModel(PacketSource.Client, packetSender);
    }

    /// <summary>
    /// Gets the packet recv sub view.
    /// </summary>
    public PacketSendSubViewModel RecvSubViewModel { get; }

    /// <summary>
    /// Gets the packet send sub view.
    /// </summary>
    public PacketSendSubViewModel SendSubViewModel { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        RecvSubViewModel.Dispose();
        SendSubViewModel.Dispose();
    }
}