//
//  FilePacketProvider.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using NosSmooth.PacketSerializer.Abstractions.Attributes;
using PacketLogger.Models.Filters;
using Remora.Results;

namespace PacketLogger.Models.Packets;

/// <summary>
/// Provides packets using a file from the file system.
/// </summary>
public class FilePacketProvider : IPacketProvider
{
    private readonly string _fileName;
    private SourceList<PacketInfo>? _packets;
    private long _index = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilePacketProvider"/> class.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    public FilePacketProvider(string fileName)
    {
        _fileName = fileName;
    }

    /// <inheritdoc />
    public string Name => Path.GetFileName(_fileName);

    /// <inheritdoc />
    public bool IsOpen => false;

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
    public SourceList<PacketInfo> Packets
        => _packets ?? throw new InvalidOperationException("File client not initialized yet.");

    /// <inheritdoc/>
    public async Task<Result> Open()
    {
        if (!File.Exists(_fileName))
        {
            return new NotFoundError($"Could not find file {_fileName}");
        }

        int successfulLines = 0;
        var packets = new SourceList<PacketInfo>();
        _index = 0;
        using var file = File.OpenRead(_fileName);
        using var fileStream = new StreamReader(file);
        if (fileStream.Peek() != '[')
        {
            return new GenericError("Looks like the file is not a packet log or in wrong format.");
        }

        while (!fileStream.EndOfStream)
        {
            var line = await fileStream.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            var splitted = line.Split('\t', 3);
            if (splitted.Length == 2)
            {
                packets.Add
                (
                    new PacketInfo
                    (
                        _index++,
                        DateTime.Now,
                        splitted[0] == "[Recv]" ? PacketSource.Server : PacketSource.Client,
                        splitted[1]
                    )
                );
                successfulLines++;
            }
            else if (splitted.Length == 3)
            {
                packets.Add
                (
                    new PacketInfo
                    (
                        _index++,
                        DateTime.Parse(splitted[0].Trim('[', ']')),
                        splitted[1] == "[Recv]" ? PacketSource.Server : PacketSource.Client,
                        splitted[2]
                    )
                );
                successfulLines++;
            }
        }

        _packets = packets;
        return Result.FromSuccess();
    }

    /// <inheritdoc/>
    public Task<Result> Close()
        => Task.FromResult(Result.FromSuccess());

    /// <inheritdoc />
    public void Clear()
    {
        // Clearing packets from file does not make any sense...
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
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public void Dispose()
    {
        _packets?.Dispose();
    }
}