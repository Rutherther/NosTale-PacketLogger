//
//  CompoundFilter.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PacketLogger.Models.Packets;

namespace PacketLogger.Models.Filters;

/// <summary>
/// A filter created from multiple filters joined by "OR".
/// </summary>
public class CompoundFilter : IFilter
{
    private readonly bool _negate;
    private readonly IFilter[] _filters;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompoundFilter"/> class.
    /// </summary>
    /// <param name="negate">Whether to negate the result (false - whitelist, true - blacklist).</param>
    /// <param name="filters">The filters to use.</param>
    public CompoundFilter(bool negate, params IFilter[] filters)
    {
        _negate = negate;
        _filters = filters;
    }

    /// <inheritdoc />
    public string FilterString => "Many";

    /// <inheritdoc />
    public bool Match(PacketInfo packet)
    {
        foreach (var filter in _filters)
        {
            if (filter.Match(packet))
            {
                return !_negate;
            }
        }

        return _negate;
    }
}