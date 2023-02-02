//
//  IPacketProvider.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using DynamicData;
using PacketLogger.Models.Filters;
using Remora.Results;

namespace PacketLogger.Models.Packets;

/// <summary>
/// Provides packets from some kind of a source.
/// </summary>
public interface IPacketProvider : INotifyPropertyChanged
{
    /// <summary>
    /// Gets whether <see cref="Open"/> was called and successfully finished.
    /// </summary>
    public bool IsOpen { get; }

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
}