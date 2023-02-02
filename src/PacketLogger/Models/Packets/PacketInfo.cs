//
//  PacketInfo.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NosSmooth.PacketSerializer.Abstractions.Attributes;

namespace PacketLogger.Models.Packets;

public record PacketInfo(long PacketIndex, DateTime Date, PacketSource Source, string PacketString);