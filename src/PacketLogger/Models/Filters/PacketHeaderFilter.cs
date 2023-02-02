//
//  PacketHeaderFilter.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using PacketLogger.Models.Packets;

namespace PacketLogger.Models.Filters;

/// <summary>
/// Filters packets that start with the given header.
/// </summary>
public class PacketHeaderFilter : IFilter
{
    private readonly string _header;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketHeaderFilter"/> class.
    /// </summary>
    /// <param name="header">The header to filter.</param>
    public PacketHeaderFilter(string header)
    {
        _header = header.Trim();
    }

    /// <inheritdoc />
    public string FilterString => _header;

    /// <inheritdoc/>
    public bool Match(PacketInfo packet)
    {
        var packetString = packet.PacketString;

        var split = ' ';
        if (packetString.StartsWith('#'))
        {
            packetString = packetString.Substring(1);
            split = '^';
        }

        var splitted = packetString.Split(split, 2);

        if (splitted.Length >= 1)
        {
            return string.Compare(splitted[0], _header, StringComparison.OrdinalIgnoreCase) == 0;
        }

        return false;
    }
}