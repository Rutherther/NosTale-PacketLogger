//
//  SettingViewModelBase.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PacketLogger.ViewModels.Settings;

/// <summary>
/// A base viewmodel class for every settings tab.
/// </summary>
public abstract class SettingViewModelBase : ViewModelBase
{
    /// <summary>
    /// Gets the name of the settings tab.
    /// </summary>
    public abstract string Name { get; }
}