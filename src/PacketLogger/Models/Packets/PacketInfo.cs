//
//  PacketInfo.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using NosSmooth.PacketSerializer.Abstractions.Attributes;

namespace PacketLogger.Models.Packets;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Fix this.")]
public record struct PacketInfo(long PacketIndex, DateTime Date, PacketSource Source, string PacketString);