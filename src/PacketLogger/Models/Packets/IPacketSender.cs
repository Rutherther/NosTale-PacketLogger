//
//  IPacketSender.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Remora.Results;

namespace PacketLogger.Models.Packets;

/// <summary>
/// A provider that may as well send or receive packets.
/// </summary>
public interface IPacketSender : IPacketProvider
{
    /// <summary>
    /// Send the given packets.
    /// </summary>
    /// <param name="packetString">The packet to send.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    Task<Result> SendPacket(string packetString, CancellationToken ct = default);

    /// <summary>
    /// Receive the given packet.
    /// </summary>
    /// <param name="packetString">The packet to send.</param>
    /// <param name="ct">The cancellation token used for cancelling the operation.</param>
    /// <returns>A result that may or may not have succeeded.</returns>
    Task<Result> ReceivePacket(string packetString, CancellationToken ct = default);
}