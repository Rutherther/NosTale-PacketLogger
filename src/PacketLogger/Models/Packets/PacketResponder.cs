//
//  PacketResponder.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using NosSmooth.Comms.Core;
using NosSmooth.Comms.Inject;
using NosSmooth.Comms.Inject.PacketResponders;
using NosSmooth.Core.Packets;
using NosSmooth.Packets;
using Remora.Results;

namespace PacketLogger.Models.Packets;

/// <inheritdoc />
public class PacketResponder : IRawPacketResponder
{
    private readonly CommsPacketProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketResponder"/> class.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public PacketResponder(CommsPacketProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public Task<Result> Respond(PacketEventArgs packetArgs, CancellationToken ct = default)
    {
        _provider.AddPacket(packetArgs);
        return Task.FromResult(Result.FromSuccess());
    }
}