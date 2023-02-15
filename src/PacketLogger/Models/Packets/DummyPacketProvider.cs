//
//  DummyPacketProvider.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using PacketLogger.Models.Filters;
using ReactiveUI;
using Remora.Results;

namespace PacketLogger.Models.Packets;

/// <inheritdoc />
public class DummyPacketProvider : IPacketProvider, IDisposable
{
    private long _index = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="DummyPacketProvider"/> class.
    /// </summary>
    /// <param name="name">The name of the tab.</param>
    public DummyPacketProvider(string name)
    {
        Name = name;
        Packets = new SourceList<PacketInfo>();
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets title of document.
    /// </summary>
    public string DocumentTitle { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool LogReceived
    {
        get => true;
        set { }
    }

    /// <inheritdoc />
    public bool LogSent
    {
        get => true;
        set { }
    }

    /// <inheritdoc />
    public bool Closed => false;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public bool IsOpen => false;

    /// <inheritdoc />
    public SourceList<PacketInfo> Packets { get; }

    /// <inheritdoc />
    public Task<Result> Open()
        => Task.FromResult(Result.FromSuccess());

    /// <inheritdoc />
    public Task<Result> Close()
        => Task.FromResult(Result.FromSuccess());

    /// <inheritdoc />
    public void Clear()
    {
        Packets.Clear();
    }

    /// <inheritdoc />
    public Task<Result> SendPacket(string packetString, CancellationToken ct = default)
    {
        Packets.Add(new PacketInfo(_index++, DateTime.Now, PacketSource.Client, packetString));
        return Task.FromResult(Result.FromSuccess());
    }

    /// <inheritdoc />
    public Task<Result> ReceivePacket(string packetString, CancellationToken ct = default)
    {
        Packets.Add(new PacketInfo(_index++, DateTime.Now, PacketSource.Server, packetString));
        return Task.FromResult(Result.FromSuccess());
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Packets.Dispose();
    }
}