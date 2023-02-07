//
//  FilterSettingsViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Reactive;
using PacketLogger.Models.Filters;
using PacketLogger.ViewModels.Filters;
using ReactiveUI;

namespace PacketLogger.ViewModels.Settings;

/// <inheritdoc />
public class FilterSettingsViewModel : SettingViewModelBase
{
    private FilterProfile _currentFilterProfile = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterSettingsViewModel"/> class.
    /// </summary>
    /// <param name="filterProfiles">The filter profiles.</param>
    public FilterSettingsViewModel(FilterProfiles filterProfiles)
    {
        Profiles = filterProfiles;
        CurrentFilterProfile = Profiles.DefaultProfile;

        AddProfile = ReactiveCommand.Create(
            () =>
            {
                var profile = new FilterProfile(false)
                {
                    Name = "New profile"
                };

                Profiles.AddProfile(profile);
            });

        RemoveCurrentProfile = ReactiveCommand.Create
        (
            () =>
            {
                var currentFilterProfile = CurrentFilterProfile;
                if (currentFilterProfile != Profiles.DefaultProfile)
                {
                    CurrentFilterProfile = Profiles.DefaultProfile;
                    Profiles.RemoveProfile(currentFilterProfile);
                }
            }
        );
    }

    /// <summary>
    /// Gets command for adding a profile.
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddProfile { get; }

    /// <summary>
    /// Gets command for removing the currently selected profile.
    /// </summary>
    public ReactiveCommand<Unit, Unit> RemoveCurrentProfile { get; }

    /// <inheritdoc />
    public override string Name => "Filters";

    /// <summary>
    /// Gets the filter profiles.
    /// </summary>
    public FilterProfiles Profiles { get; }

    /// <summary>
    /// Gets the current filter profile.
    /// </summary>
    public FilterProfile CurrentFilterProfile
    {
        get => _currentFilterProfile;
        set
        {
            _currentFilterProfile = value;
            CurrentFilterProfileViewModel = new FilterConfigViewModel(_currentFilterProfile);
        }
    }

    /// <summary>
    /// Gets the current filter profile view model.
    /// </summary>
    public FilterConfigViewModel CurrentFilterProfileViewModel { get; private set; } = null!;
}