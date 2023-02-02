//
//  NostaleProcesses.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NosSmooth.Comms.Local;
using NosSmooth.Core.Extensions;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Options;
using ReactiveUI;

namespace PacketLogger.Models;

/// <summary>
/// Keeps and refreshes a collection of NosTale processes.
/// </summary>
public class NostaleProcesses : IDisposable
{
    private readonly IDisposable _cleanUp;
    private readonly List<long> _errorfulProcesses;

    /// <summary>
    /// Initializes a new instance of the <see cref="NostaleProcesses"/> class.
    /// </summary>
    public NostaleProcesses()
    {
        _errorfulProcesses = new List<long>();
        Processes = new ObservableCollection<NostaleProcess>();
        _cleanUp = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Subscribe
            (
                _ =>
                {
                    try
                    {
                        Refresh().GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            );
    }

    /// <summary>
    /// Gets NosTale processes.
    /// </summary>
    public ObservableCollection<NostaleProcess> Processes { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        _cleanUp.Dispose();
    }

    private async Task Refresh()
    {
        var nosTaleProcesses = CommsInjector.FindNosTaleProcesses().Select(x => x.Id).ToArray();
        var toRemove = new List<NostaleProcess>();

        foreach (var currentProcess in Processes)
        {
            if (nosTaleProcesses.All(x => x != currentProcess.Process.Id))
            {
                toRemove.Add(currentProcess);
            }
            else
            {
                RxApp.MainThreadScheduler.Schedule(() => currentProcess.ObserveChanges());
            }
        }

        foreach (var remove in toRemove)
        {
            RxApp.MainThreadScheduler.Schedule(() => Processes.Remove(remove));
        }

        foreach (var openProcess in nosTaleProcesses)
        {
            if (Processes.All(x => x.Process.Id != openProcess) && !_errorfulProcesses.Contains(openProcess))
            {
                var process = Process.GetProcessById(openProcess);
                NosBrowserManager nosBrowserManager = new NosBrowserManager
                (
                    process,
                    new PlayerManagerOptions(),
                    new SceneManagerOptions(),
                    new PetManagerOptions(),
                    new NetworkManagerOptions(),
                    new UnitManagerOptions()
                );
                var result = nosBrowserManager.Initialize();
                if (result.IsSuccess)
                {
                    RxApp.MainThreadScheduler.Schedule
                        (() => Processes.Add(new NostaleProcess(process, nosBrowserManager)));
                }
                else
                {
                    _errorfulProcesses.Add(openProcess);
                    Console.WriteLine(result.ToFullString());
                }
            }
        }

        var errorfulToRemove = new List<long>();

        foreach (var errorfulProcess in _errorfulProcesses)
        {
            if (nosTaleProcesses.All(x => x != errorfulProcess))
            {
                errorfulToRemove.Add(errorfulProcess);
            }
        }

        foreach (var errorfulRemove in errorfulToRemove)
        {
            _errorfulProcesses.Remove(errorfulRemove);
        }
    }
}