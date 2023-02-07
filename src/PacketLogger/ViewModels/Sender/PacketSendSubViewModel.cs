//
//  PacketSendSubViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using PacketLogger.Models.Packets;
using ReactiveUI;

namespace PacketLogger.ViewModels.Sender;

/// <inheritdoc />
public class PacketSendSubViewModel : ViewModelBase, IDisposable
{
    private readonly IPacketProvider _sender;
    private string[]? _cachedPacketData;
    private IDisposable? _sendingTask;
    private SemaphoreSlim _semaphore;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketSendSubViewModel"/> class.
    /// </summary>
    /// <param name="source">The packet source to use.</param>
    /// <param name="sender">The sender to send packets to.</param>
    public PacketSendSubViewModel(PacketSource source, IPacketProvider sender)
    {
        _semaphore = new SemaphoreSlim(1, 1);
        Source = source;
        _sender = sender;

        SendPackets = ReactiveCommand.CreateFromTask(SendPacketData);
        ToggleRepetetiveSend = ReactiveCommand.Create(
            () =>
            {
                if (IsSending)
                {
                    _semaphore.Wait();
                    _sendingTask?.Dispose();
                    _sendingTask = null;
                    _semaphore.Release();
                }
                else
                {
                    _semaphore.Wait();
                    _cachedPacketData = null;
                    _sendingTask?.Dispose();
                    _sendingTask = Observable.Timer(DateTimeOffset.Now, TimeSpan.FromMilliseconds(RepetitionDelay))
                        .Subscribe
                        (
                            _ =>
                            {
                                SendPacketData().GetAwaiter().GetResult();
                            }
                        );
                    _semaphore.Release();
                }

                IsSending = !IsSending;
            });
    }

    /// <summary>
    /// Gets the source to send the packets as.
    /// </summary>
    public PacketSource Source { get; }

    /// <summary>
    /// Gets or sets whether current repetetively sending.
    /// </summary>
    public bool IsSending { get; private set; }

    /// <summary>
    /// Gets or sets the packets to send separated by a line.
    /// </summary>
    public string PacketsData { get; set; } = string.Empty;

    /// <summary>
    /// The delay of repetition in milliseconds.
    /// </summary>
    public int RepetitionDelay { get; set; } = 100;

    /// <summary>
    /// Gets or sets the command used to send the packets.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SendPackets { get; }

    /// <summary>
    /// Gets the command used for toggling repetetive send.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleRepetetiveSend { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        _sendingTask?.Dispose();
        SendPackets.Dispose();
        ToggleRepetetiveSend.Dispose();
    }

    private async Task SendPacketData()
    {
        if (!IsSending || _cachedPacketData is null)
        {
            _cachedPacketData = PacketsData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }

        foreach (var line in _cachedPacketData)
        {
            await Send(line);
        }
    }

    private Task Send(string packetString)
    {
        if (Source == PacketSource.Server)
        {
            return _sender.ReceivePacket(packetString);
        }
        else
        {
            return _sender.SendPacket(packetString);
        }
    }
}