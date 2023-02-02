//
//  StartsWithFilter.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PacketLogger.Models.Packets;

namespace PacketLogger.Models.Filters;

/// <summary>
/// Filters packets that start with the given string.
/// </summary>
public class StartsWithFilter : IFilter
{
    private string _startsWith;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartsWithFilter"/> class.
    /// </summary>
    /// <param name="startsWith">The string to filter.</param>
    public StartsWithFilter(string startsWith)
    {
        _startsWith = startsWith;
    }

    /// <inheritdoc />
    public string FilterString => _startsWith;

    /// <inheritdoc />
    public bool Match(PacketInfo packet)
    {
        return packet.PacketString.StartsWith(_startsWith);
    }
}