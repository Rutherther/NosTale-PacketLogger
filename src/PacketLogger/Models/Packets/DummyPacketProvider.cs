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
    /// <summary>
    /// Initializes a new instance of the <see cref="DummyPacketProvider"/> class.
    /// </summary>
    public DummyPacketProvider()
    {
        var index = 0;
        Packets = new SourceList<PacketInfo>();
        Packets.Add(new PacketInfo(index++, DateTime.Now, PacketSource.Client, "#cl"));
        Packets.Add(new PacketInfo(index++, DateTime.Now, PacketSource.Client, "cl"));
        for (var i = 0; i < 1000; i++)
        {
            Packets.Add
                (new PacketInfo(index++, DateTime.Now.AddSeconds(-1000 + i), PacketSource.Client, "walk 10 10"));
            Packets.Add
                (new PacketInfo(index++, DateTime.Now.AddSeconds(-1000 + i), PacketSource.Server, "mv 1 50 52 123 123 89012390812 189023 182309 1823 189023 901283 091823 091823 901823 901283 091283 019283901283 901283 901 2831290 812390128390128213908139012839012839012390128390128938120938 1290 3190 adsadf"));
            Packets.Add
                (new PacketInfo(index++, DateTime.Now.AddSeconds(-1000 + i), PacketSource.Client, "walk 12 14"));
            Packets.Add
                (new PacketInfo(index++, DateTime.Now.AddSeconds(-1000 + i), PacketSource.Server, "mv 1 48 43"));
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

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
    public bool IsOpen => false;

    /// <inheritdoc />
    public SourceList<PacketInfo> Packets { get; }

    /// <inheritdoc />
    public Task<Result> Open()
    {
        return Task.FromResult(Result.FromSuccess());
    }

    /// <inheritdoc />
    public Task<Result> Close()
    {
        return Task.FromResult(Result.FromSuccess());
    }

    /// <inheritdoc />
    public void Clear()
    {
        Packets.Clear();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Packets.Dispose();
    }
}