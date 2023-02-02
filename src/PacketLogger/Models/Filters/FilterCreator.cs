//
//  FilterCreator.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace PacketLogger.Models.Filters;

/// <summary>
/// A builder/creator of <see cref="IFilter"/>.
/// </summary>
public static class FilterCreator
{
    /// <summary>
    /// A type of a filter.
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// <see cref="PacketHeaderFilter"/>.
        /// </summary>
        PacketHeader,

        /// <summary>
        /// <see cref="ContainsFilter"/>.
        /// </summary>
        Contains,

        /// <summary>
        /// <see cref="StartsWithFilter"/>.
        /// </summary>
        StartsWith
    }

    /// <summary>
    /// Information about a filter to create.
    /// </summary>
    /// <param name="Type">The type of the filter.</param>
    /// <param name="Value">The value.</param>
    public record FilterData(FilterType Type, string Value);

    /// <summary>
    /// Build a filter of the given type and value.
    /// </summary>
    /// <param name="type">The type of the filter to build.</param>
    /// <param name="value">The value of the filter.</param>
    /// <returns>A filter.</returns>
    public static IFilter BuildFilter(FilterType type, string value)
    {
        switch (type)
        {
            case FilterType.Contains:
                return new ContainsFilter(value);
            case FilterType.PacketHeader:
                return new PacketHeaderFilter(value);
            case FilterType.StartsWith:
                return new StartsWithFilter(value);
        }

        throw new UnreachableException();
    }
}