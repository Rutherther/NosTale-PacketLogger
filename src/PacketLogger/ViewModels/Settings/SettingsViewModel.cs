//
//  SettingsViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using PacketLogger.Models.Filters;

namespace PacketLogger.ViewModels.Settings;

/// <inheritdoc />
public class SettingsViewModel : ViewModelBase
{
    private readonly FilterProfiles _filterProfiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="filterProfiles">The filter profiles.</param>
    public SettingsViewModel(FilterProfiles filterProfiles)
    {
        _filterProfiles = filterProfiles;
        Settings = new[]
        {
            new FilterSettingsViewModel(filterProfiles)
        };

        SelectedSetting = Settings.First();
    }

    /// <summary>
    /// Gets the setting tabs list.
    /// </summary>
    public IReadOnlyList<SettingViewModelBase> Settings { get; }

    /// <summary>
    /// Gets the currently selected tab.
    /// </summary>
    public SettingViewModelBase SelectedSetting { get; }
}