//
//  ClientPacketProvider.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using NosSmooth.Comms.Local;
using NosSmooth.Core.Client;
using NosSmooth.Core.Packets;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using ReactiveUI;
using Remora.Results;

namespace PacketLogger.Models.Packets;

/// <summary>
/// A packet provider using <see cref="INostaleClient"/>.
/// </summary>
public abstract class ClientPacketProvider : ReactiveObject, IPacketProvider
{
    private readonly IDisposable _cleanUp;
    private readonly NostaleProcess _process;
    private readonly INostaleClient _client;
    private readonly CancellationTokenSource _ctSource;
    private long _currentIndex;
    private Task<Result>? _runTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientPacketProvider"/> class.
    /// </summary>
    /// <param name="process">The process.</param>
    /// <param name="client">The nostale client.</param>
    public ClientPacketProvider(NostaleProcess process, INostaleClient client)
    {
        _ctSource = new CancellationTokenSource();
        _process = process;
        _client = client;
        Packets = new SourceList<PacketInfo>();
        var cleanUp1 = process.WhenPropertyChanged(x => x.CharacterString)
            .Subscribe
            (
                _ => this.RaisePropertyChanged(nameof(Name))
            );

        var cleanUp2 = process.WhenAnyValue(x => x.Closed)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(Closed)));
        _cleanUp = new CompositeDisposable(cleanUp1, cleanUp2);
    }

    /// <inheritdoc />
    public string Name => (_process.BrowserManager.IsInGame.Get()
        ? _process.BrowserManager.PlayerManager.Get().Player.Name
        : null) ?? $"Not in game ({_process.Process.Id})";

    /// <summary>
    /// Gets whether the process has been closed.
    /// </summary>
    public bool Closed => _process.Closed;

    /// <summary>
    /// Gets or sets title of document.
    /// </summary>
    public string DocumentTitle { get; set; } = string.Empty;

    /// <inheritdoc />
    public abstract bool IsOpen { get; }

    /// <inheritdoc />
    public SourceList<PacketInfo> Packets { get; }

    /// <inheritdoc />
    public bool LogReceived { get; set; } = true;

    /// <inheritdoc />
    public bool LogSent { get; set; } = true;

    /// <inheritdoc />
    public Task<Result> Open()
    {
        _runTask = Task.Run(() => _client.RunAsync(_ctSource.Token));
        return Task.FromResult(Result.FromSuccess());
    }

    /// <inheritdoc />
    public virtual Task<Result> Close()
    {
        _ctSource.Cancel();
        if (_runTask is not null)
        {
            return _runTask;
        }

        return Task.FromResult(Result.FromSuccess());
    }

    /// <inheritdoc />
    public void Clear()
    {
        Packets.Clear();
    }

    /// <inheritdoc />
    public Task<Result> SendPacket(string packetString, CancellationToken ct = default)
        => _client.SendPacketAsync(packetString, ct);

    /// <inheritdoc />
    public Task<Result> ReceivePacket(string packetString, CancellationToken ct = default)
        => _client.ReceivePacketAsync(packetString, ct);

    /// <summary>
    /// Add the given packets from an event.
    /// </summary>
    /// <param name="packetArgs">The packet event args.</param>
    internal void AddPacket(PacketEventArgs packetArgs)
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
    }

    /// <summary>
    /// A dispose used instead of <see cref="Dispose"/>
    /// to prevent the service provider disposing.
    /// </summary>
    public void CustomDispose()
    {
        _ctSource.Dispose();
        _cleanUp.Dispose();
        Packets.Dispose();
    }
}