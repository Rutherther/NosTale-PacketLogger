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
using NosSmooth.Comms.Local;
using NosSmooth.Core.Packets;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using Remora.Results;

namespace PacketLogger.Models.Packets;

/// <summary>
/// A packet provider using a connection to a nostale client.
/// </summary>
public class CommsPacketProvider : IPacketSender
{
    private readonly Comms _comms;
    private long _currentIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommsPacketProvider"/> class.
    /// </summary>
    /// <param name="comms">The comms.</param>
    public CommsPacketProvider(Comms comms)
    {
        _comms = comms;
        Packets = new SourceList<PacketInfo>();
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public bool IsOpen => _comms.Connection.Connection.State == ConnectionState.Open;

    /// <inheritdoc />
    public SourceList<PacketInfo> Packets { get; }

    /// <inheritdoc />
    public bool LogReceived { get; set; } = true;

    /// <inheritdoc />
    public bool LogSent { get; set; } = true;

    /// <inheritdoc />
    public Task<Result> Open()
        => Task.FromResult(Result.FromSuccess());

    /// <inheritdoc />
    public Task<Result> Close()
    {
        _comms.Connection.Connection.Disconnect();
        return Task.FromResult(Result.FromSuccess());
    }

    /// <inheritdoc />
    public void Clear()
    {
        Packets.Clear();
    }

    /// <inheritdoc />
    public Task<Result> SendPacket(string packetString, CancellationToken ct = default)
        => _comms.Client.SendPacketAsync(packetString, ct);

    /// <inheritdoc />
    public Task<Result> ReceivePacket(string packetString, CancellationToken ct = default)
        => _comms.Client.ReceivePacketAsync(packetString, ct);

    /// <summary>
    /// Add the given packets from an event.
    /// </summary>
    /// <param name="packetArgs">The packet event args.</param>
    /// <typeparam name="TPacket">The type of the deserialized packet.</typeparam>
    internal void AddPacket<TPacket>(PacketEventArgs<TPacket> packetArgs)
    {
        var index = Interlocked.Increment(ref _currentIndex);
        if ((packetArgs.Source == PacketSource.Server && LogReceived)
            || (packetArgs.Source == PacketSource.Client && LogSent))
        {
            Packets.Add(new PacketInfo(index, DateTime.Now, packetArgs.Source, packetArgs.PacketString));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Packets.Dispose();
    }
}