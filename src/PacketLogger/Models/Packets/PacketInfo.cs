//
//  PacketInfo.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using NosSmooth.PacketSerializer.Abstractions.Attributes;

namespace PacketLogger.Models.Packets;

/// <summary>
/// Information about a received or sent packet.
/// </summary>
/// <param name="PacketIndex">The index of the packet used for sorting.</param>
/// <param name="Date">The time the packet was received at.</param>
/// <param name="Source">The source of the packet.</param>
/// <param name="PacketString">The packet.</param>
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Fix this.")]
public record struct PacketInfo(long PacketIndex, DateTime Date, PacketSource Source, string PacketString);