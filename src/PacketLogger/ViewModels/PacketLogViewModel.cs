//
//  PacketLogViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Collections;
using DynamicData;
using DynamicData.Binding;
using PacketLogger.Models;
using PacketLogger.Models.Filters;
using PacketLogger.Models.Packets;
using ReactiveUI;
using Reloaded.Memory.Kernel32;

namespace PacketLogger.ViewModels;

/// <inheritdoc />
public class PacketLogViewModel : ViewModelBase, IDisposable
{
    private readonly ReadOnlyObservableCollection<PacketInfo> _packets;
    private readonly IDisposable _cleanUp;
    private bool _logReceived = true;
    private bool _logSent = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketLogViewModel"/> class.
    /// </summary>
    /// <param name="packetProvider">The packet provider.</param>
    public PacketLogViewModel(IPacketProvider packetProvider)
    {
        Provider = packetProvider;

        var dynamicFilter = this.WhenValueChanged(@this => @this.CurrentFilter)
            .Select
            (
                filter =>
                {
                    return (Func<PacketInfo, bool>)((pi) =>
                    {
                        if (filter is null)
                        {
                            return true;
                        }

                        return filter.Match(pi);
                    });
                }
            );

        var packetsSubscription = Provider.Packets.Connect()
            .Filter(dynamicFilter)
            .Sort(new PacketComparer())
            .Bind(out _packets)
            .ObserveOn(RxApp.MainThreadScheduler)
            .DisposeMany()
            .Subscribe
            (
                _ =>
                {
                    if (Scroll)
                    {
                        RxApp.MainThreadScheduler.Schedule
                            (DateTimeOffset.Now.AddMilliseconds(100), () =>
                                {
                                    if (FilteredPackets.Count > 0)
                                    {
                                        SelectedPacket = FilteredPackets[^1];
                                    }
                                }
                            );
                    }
                }
            );

        _cleanUp = packetsSubscription;
        CopyPackets = ReactiveCommand.CreateFromObservable<IList, Unit>
        (
            list => Observable.StartAsync
            (
                async () =>
                {
                    var clipboardString = string.Join
                        ('\n', list.OfType<PacketInfo>().Select(x => x.PacketString));
                    await Application.Current!.Clipboard!.SetTextAsync(clipboardString);
                }
            )
        );

        TogglePane = ReactiveCommand.Create<Unit, bool>
        (
            _ => PaneOpen = !PaneOpen
        );

        Clear = ReactiveCommand.Create
        (
            () => Provider.Clear()
        );

        SendFilter = new();
        RecvFilter = new();

        SendFilter.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "Whitelist")
            {
                CreateSendRecv();
            }
        };
        RecvFilter.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "Whitelist")
            {
                CreateSendRecv();
            }
        };
        SendFilter.Filters.CollectionChanged += (s, e) => { CreateSendRecv(); };
        RecvFilter.Filters.CollectionChanged += (s, e) => { CreateSendRecv(); };
    }

    /// <summary>
    /// Gets the send filter model.
    /// </summary>
    public PacketLogFilterViewModel SendFilter { get; }

    /// <summary>
    /// Gets the receive filter model.
    /// </summary>
    public PacketLogFilterViewModel RecvFilter { get; }

    /// <summary>
    /// Gets the currently applied filter.
    /// </summary>
    public IFilter? CurrentFilter { get; private set; }

    /// <summary>
    /// Gets the filtered packets.
    /// </summary>
    public ReadOnlyObservableCollection<PacketInfo> FilteredPackets => _packets;

    /// <summary>
    /// Gets packet provider.
    /// </summary>
    public IPacketProvider Provider { get; }

    /// <summary>
    /// Gets whether the pane is open.
    /// </summary>
    public bool PaneOpen { get; private set; } = true;

    /// <summary>
    /// Gets the toggle pane command.
    /// </summary>
    public ReactiveCommand<Unit, bool> TogglePane { get; }

    /// <summary>
    /// Gets command to copy packets.
    /// </summary>
    public ReactiveCommand<IList, Unit> CopyPackets { get; }

    /// <summary>
    /// Gets the command for clearing.
    /// </summary>
    public ReactiveCommand<Unit, Unit> Clear { get; }

    /// <summary>
    /// Gets or sets whether to log received packets.
    /// </summary>
    public bool LogReceived
    {
        get => _logReceived;
        set
        {
            Provider.LogReceived = value;
            _logReceived = value;
        }
    }

    /// <summary>
    /// Gets or sets whether to log sent packets.
    /// </summary>
    public bool LogSent
    {
        get => _logSent;
        set
        {
            Provider.LogSent = value;
            _logSent = value;
        }
    }

    /// <summary>
    /// Gets or sets whether to scroll to teh bottom of the grid.
    /// </summary>
    public bool Scroll { get; set; } = true;

    /// <summary>
    /// Gets or sets the currently selected packet.
    /// </summary>
    public object? SelectedPacket { get; set; }

    /// <summary>
    /// Gets empty string.
    /// </summary>
    public string Empty { get; } = string.Empty;

    private void CreateSendRecv()
    {
        IFilter recvFilter = CreateCompound(RecvFilter);
        IFilter sendFilter = CreateCompound(SendFilter);

        CurrentFilter = new SendRecvFilter(sendFilter, recvFilter);
    }

    private IFilter CreateCompound(PacketLogFilterViewModel packetLogFilter)
    {
        List<IFilter> filters = new List<IFilter>();

        foreach (var filter in packetLogFilter.Filters)
        {
            filters.Add(FilterCreator.BuildFilter(filter.Type, filter.Value));
        }

        return new CompoundFilter(!packetLogFilter.Whitelist, filters.ToArray());
    }

    /// <inheritdoc />
    public void Dispose()
    {
        TogglePane.Dispose();
        CopyPackets.Dispose();
        Clear.Dispose();
        Provider.Dispose();
        (Provider as CommsPacketProvider)?.CustomDispose();
        _cleanUp.Dispose();

        SendFilter.Dispose();
        RecvFilter.Dispose();
    }
}