//
//  PcapPacketProvider.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using NosSmooth.Core.Client;
using ReactiveUI;
using Remora.Results;

namespace PacketLogger.Models.Packets;

/// <summary>
/// A packet provider using pcap.
/// </summary>
public class PcapPacketProvider : ClientPacketProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PcapPacketProvider"/> class.
    /// </summary>
    /// <param name="process">The nostale process.</param>
    /// <param name="client">The pcap client.</param>
    public PcapPacketProvider(NostaleProcess process, INostaleClient client)
        : base(process, client)
    {
    }

    /// <inheritdoc />
    public override bool IsOpen => true;
}