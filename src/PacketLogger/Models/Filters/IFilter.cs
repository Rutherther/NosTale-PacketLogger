//
//  IFilter.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PacketLogger.Models.Packets;

namespace PacketLogger.Models.Filters;

/// <summary>
/// Filter a packet.
/// </summary>
public interface IFilter
{
    /// <summary>
    /// The string to filter.
    /// </summary>
    public string FilterString { get; }

    /// <summary>
    /// Match the given packet.
    /// </summary>
    /// <param name="packet">The packet info.</param>
    /// <returns>Whether to keep the packet.</returns>
    public bool Match(PacketInfo packet);
}