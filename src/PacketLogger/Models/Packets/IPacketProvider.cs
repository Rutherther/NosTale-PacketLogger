//
//  IPacketProvider.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using PacketLogger.Models.Filters;
using PacketLogger.Models.Titles;
using Remora.Results;

namespace PacketLogger.Models.Packets;

/// <summary>
/// Provides packets from some kind of a source.
/// </summary>
public interface IPacketProvider : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets title of document, if any.
    /// </summary>
    public string DocumentTitle { get; set; }

    /// <summary>
    /// Gets whether <see cref="Open"/> was called and successfully finished.
    /// </summary>
    public bool IsOpen { get; }

    /// <summary>
    /// Gets or sets whether to log received packets.
    /// </summary>
    public bool LogReceived { get; set; }

    /// <summary>
    /// Gets or sets whether to log sent pckets.
    /// </summary>
    public bool LogSent { get; set; }

    /// <summary>
    /// Gets the filtered packets from this provider.
    /// </summary>
    public SourceList<PacketInfo> Packets { get; }

    /// <summary>
    /// Open the provider/connection, load etc.
    /// </summary>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Task<Result> Open();

    /// <summary>
    /// Close the connection, dispose.
    /// </summary>
    /// <returns>A result that may or may not have succeeded.</returns>
    public Task<Result> Close();

    /// <summary>
    /// Clear all packets.
    /// </summary>
    public void Clear();

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