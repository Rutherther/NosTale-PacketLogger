//
//  PacketSourceConverter.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using NosSmooth.PacketSerializer.Abstractions.Attributes;

namespace PacketLogger.Converters;

/// <summary>
/// Converts <see cref="PacketSource"/>.
/// </summary>
public class PacketSourceConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert
    (
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (value is not PacketSource source)
        {
            throw new ArgumentException("Must be PacketSource.", nameof(value));
        }

        return source == PacketSource.Client ? "Send" : "Recv";
    }

    /// <inheritdoc />
    public object? ConvertBack
    (
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (value is not string stringVal)
        {
            throw new ArgumentException("Must be string.", nameof(value));
        }

        if (stringVal == "Send")
        {
            return PacketSource.Client;
        }
        if (stringVal == "Recv")
        {
            return PacketSource.Server;
        }

        throw new ArgumentException("Must be \"Recv\" or \"Send\".");
    }
}