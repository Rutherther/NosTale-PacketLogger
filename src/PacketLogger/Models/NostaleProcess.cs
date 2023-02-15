//
//  NostaleProcess.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NosSmooth.LocalBinding;
using ReactiveUI;
using Reloaded.Memory.Exceptions;

namespace PacketLogger.Models;

/// <summary>
/// A NosTale process.
/// </summary>
public class NostaleProcess : ObservableObject
{
    private bool _wasInGame;
    private string? _lastCharacterName;

    /// <summary>
    /// Initializes a new instance of the <see cref="NostaleProcess"/> class.
    /// </summary>
    /// <param name="process">The process.</param>
    /// <param name="browserManager">The browser manager.</param>
    public NostaleProcess(Process process, NosBrowserManager browserManager)
    {
        Process = process;
        BrowserManager = browserManager;

        ObserveChanges();
    }

    /// <summary>
    /// Gets the name of the character.
    /// </summary>
    public string CharacterString => _lastCharacterName ?? "- (Not in game)";

    /// <summary>
    /// Gets the process string.
    /// </summary>
    public string ProcessString => $"{Process.Id} | {Process.ProcessName}";

    /// <summary>
    /// Gets the process.
    /// </summary>
    public Process Process { get; init; }

    /// <summary>
    /// Gets the browser manager.
    /// </summary>
    public NosBrowserManager BrowserManager { get; init; }

    /// <summary>
    /// Gets whether the process has been closed.
    /// </summary>
    public bool Closed { get; private set; }

    /// <summary>
    /// Look for changes in the process, fire property changed.
    /// </summary>
    internal void ObserveChanges()
    {
        try
        {
            if (Process.HasExited)
            {
                Closed = true;
                return;
            }

            if (BrowserManager.IsInGame.Get() != _wasInGame)
            {
                OnPropertyChanging(nameof(BrowserManager));
                OnPropertyChanged(nameof(BrowserManager));
            }

            var currentCharacterName = BrowserManager.IsInGame.Get() ? BrowserManager.PlayerManager.Get().Player.Name : null;
            var changed = _lastCharacterName != currentCharacterName;

            if (changed)
            {
                OnPropertyChanging(nameof(CharacterString));
            }

            _wasInGame = BrowserManager.IsInGame.Get();
            _lastCharacterName = currentCharacterName;

            if (changed)
            {
                OnPropertyChanged(nameof(CharacterString));
            }
        }
        catch (MemoryPermissionException)
        {
            // ignored, wait a bit, NosTale is probably just starting.
        }
    }
}