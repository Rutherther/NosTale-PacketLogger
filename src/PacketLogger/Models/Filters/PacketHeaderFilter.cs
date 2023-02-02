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
    private readonly string _headerFilter;
    private readonly string _headerFilterReturn;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketHeaderFilter"/> class.
    /// </summary>
    /// <param name="header">The header to filter.</param>
    public PacketHeaderFilter(string header)
    {
        _header = header.Trim();
        _headerFilter = _header;
        _headerFilterReturn = "#" + _header;
    }

    /// <inheritdoc />
    public string FilterString => _header;

    /// <inheritdoc/>
    public bool Match(PacketInfo packet)
    {
        return packet.PacketString.StartsWith
            (_header, StringComparison.OrdinalIgnoreCase) || packet.PacketString.StartsWith
            (_headerFilterReturn, StringComparison.OrdinalIgnoreCase);
    }
}