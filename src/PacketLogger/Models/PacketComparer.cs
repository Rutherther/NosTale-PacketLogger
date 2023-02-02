//
//  PacketComparer.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using PacketLogger.Models.Packets;

namespace PacketLogger.Models;

/// <inheritdoc />
public class PacketComparer : IComparer<PacketInfo>
{
    /// <inheritdoc />
    public int Compare(PacketInfo x, PacketInfo y)
    {
        return x.PacketIndex.CompareTo(y.PacketIndex);
    }
}