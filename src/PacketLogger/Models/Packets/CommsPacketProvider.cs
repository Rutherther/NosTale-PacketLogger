//
//  CommsPacketProvider.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using NosSmooth.Comms.Local;
using NosSmooth.Core.Packets;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using ReactiveUI;
using Remora.Results;

namespace PacketLogger.Models.Packets;

/// <summary>
/// A packet provider using a connection to a nostale client.
/// </summary>
public class CommsPacketProvider : ClientPacketProvider
{
    private readonly Comms _comms;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommsPacketProvider"/> class.
    /// </summary>
    /// <param name="process">The process.</param>
    /// <param name="comms">The comms.</param>
    public CommsPacketProvider(NostaleProcess process, Comms comms)
        : base(process, comms.Client)
    {
        _comms = comms;
    }

    /// <inheritdoc />
    public override bool IsOpen => _comms.Connection.Connection.State == ConnectionState.Open;

    /// <inheritdoc />
    public override Task<Result> Close()
    {
        _comms.Connection.Connection.Disconnect();
        return Task.FromResult(Result.FromSuccess());
    }
}