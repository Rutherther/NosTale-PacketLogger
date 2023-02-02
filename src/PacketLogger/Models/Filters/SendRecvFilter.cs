//
//  SendRecvFilter.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NosSmooth.PacketSerializer.Abstractions.Attributes;
using PacketLogger.Models.Packets;

namespace PacketLogger.Models.Filters;

/// <summary>
/// Filter using different matches for sent and received packets.
/// </summary>
public class SendRecvFilter : IFilter
{
    private readonly IFilter _sendFilter;
    private readonly IFilter _recvFilter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendRecvFilter"/> class.
    /// </summary>
    /// <param name="sendFilter">The filter for sent packets.</param>
    /// <param name="recvFilter">The filter for received packets.</param>
    public SendRecvFilter(IFilter sendFilter, IFilter recvFilter)
    {
        _sendFilter = sendFilter;
        _recvFilter = recvFilter;
    }

    /// <inheritdoc />
    public string FilterString => "SendRecv";

    /// <inheritdoc />
    public bool Match(PacketInfo packet)
    {
        if (packet.Source == PacketSource.Server)
        {
            return _recvFilter.Match(packet);
        }

        return _sendFilter.Match(packet);
    }
}