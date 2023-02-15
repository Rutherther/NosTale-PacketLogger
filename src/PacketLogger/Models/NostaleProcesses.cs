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
using System.Management;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using NosSmooth.Comms.Local;
using NosSmooth.Core.Extensions;
using NosSmooth.LocalBinding;
using NosSmooth.LocalBinding.Errors;
using NosSmooth.LocalBinding.Options;
using NosSmooth.LocalBinding.Structs;
using ReactiveUI;

namespace PacketLogger.Models;

/// <summary>
/// Keeps and refreshes a collection of NosTale processes.
/// </summary>
public class NostaleProcesses : IDisposable
{
    private readonly IDisposable? _cleanUp;
    private readonly SemaphoreSlim _semaphore;
    private readonly ManagementEventWatcher? _processStartWatcher;
    private readonly ManagementEventWatcher? _processStopWatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="NostaleProcesses"/> class.
    /// </summary>
    public NostaleProcesses()
    {
        _semaphore = new SemaphoreSlim(1, 1);
        Processes = new ObservableCollection<NostaleProcess>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                _cleanUp = Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(1))
                    .Subscribe(_ => UpdateNames());

                Supported = true;
                _processStartWatcher = new ManagementEventWatcher
                    (new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
                _processStartWatcher.EventArrived += HandleProcessOpenedEvent;
                _processStartWatcher.Start();

                _processStopWatcher = new ManagementEventWatcher
                    (new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
                _processStopWatcher.EventArrived += HandleProcessClosedEvent;
                _processStopWatcher.Start();

                // initial nostale processes
                // rest is handled by events
                foreach (var process in CommsInjector.FindNosTaleProcesses())
                {
                    HandleProcessOpened(process.Id);
                }
            }
        }
    }

    private void HandleProcessOpenedEvent(object sender, EventArrivedEventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            HandleProcessOpened(Convert.ToInt32(e.NewEvent.Properties["ProcessId"].Value));
        }
    }

    private void HandleProcessClosedEvent(object sender, EventArrivedEventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            HandleProcessClosed(Convert.ToInt32(e.NewEvent.Properties["ProcessId"].Value));
        }
    }

    private void HandleProcessOpened(int processId)
    {
        Process process;
        try
        {
            process = Process.GetProcessById(processId);
        }
        catch (Exception)
        {
            return;
        }

        if (!NosBrowserManager.IsProcessNostaleProcess(process))
        {
            return;
        }

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
        if (!result.IsSuccess)
        {
            Console.WriteLine
                ($"Got an error when trying to initialize nos browser manager for {process.ProcessName}");
            Console.WriteLine(result.ToFullString());
        }

        if (nosBrowserManager.IsModuleLoaded<PlayerManager>())
        {
            RxApp.MainThreadScheduler.Schedule
                (() =>
                    {
                        _semaphore.Wait();
                        Processes.Add(new NostaleProcess(process, nosBrowserManager));
                        _semaphore.Release();
                    }
                );
        }
        else
        {
            Console.WriteLine
            (
                $"Cannot add {process.ProcessName} to nostale processes as player manager was not found in memory."
            );
        }
    }

    private void HandleProcessClosed(int processId)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var process = Processes.FirstOrDefault(x => x.Process.Id == processId);

            if (process is not null)
            {
                RxApp.MainThreadScheduler.Schedule
                    (() =>
                        {
                            process.ObserveChanges();
                            _semaphore.Wait();
                            Processes.Remove(process);
                            _semaphore.Release();
                        }
                    );
            }
        }
    }

    /// <summary>
    /// Gets whether tracking and attaching to processes is supported for this run.
    /// </summary>
    /// <remarks>
    /// Supported only on Windows run as elevated.
    /// </remarks>
    public bool Supported { get; }

    /// <summary>
    /// Gets NosTale processes.
    /// </summary>
    public ObservableCollection<NostaleProcess> Processes { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _processStartWatcher?.Stop();
            _processStartWatcher?.Dispose();
            _processStopWatcher?.Stop();
            _processStopWatcher?.Dispose();
        }
    }

    private void UpdateNames()
    {
        _semaphore.Wait();
        foreach (var process in Processes)
        {
            process.ObserveChanges();
        }
        _semaphore.Release();
    }
}