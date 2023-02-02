//
//  ContainsFilter.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PacketLogger.Models.Packets;

namespace PacketLogger.Models.Filters;

/// <summary>
/// Filters packets that contain the given string.
/// </summary>
public class ContainsFilter : IFilter
{
    private readonly string _contains;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainsFilter"/> class.
    /// </summary>
    /// <param name="contains">The string to filter.</param>
    public ContainsFilter(string contains)
    {
        _contains = contains;
    }

    /// <inheritdoc />
    public string FilterString => _contains;

    /// <inheritdoc />
    public bool Match(PacketInfo packet)
    {
        return packet.PacketString.Contains(_contains, StringComparison.OrdinalIgnoreCase);
    }
}