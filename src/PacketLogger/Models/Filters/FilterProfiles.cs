//
//  FilterProfiles.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace PacketLogger.Models.Filters;

/// <summary>
/// A collection of <see cref="FilterProfile"/>s.
/// </summary>
public class FilterProfiles
{
    private bool _defaultFilterEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterProfiles"/> class.
    /// </summary>
    public FilterProfiles()
    {
        DefaultProfile = new FilterProfile(true)
        {
            Name = "Default"
        };

        AllProfiles = new ObservableCollection<FilterProfile>();
        SelectableProfiles = new ObservableCollection<FilterProfile>();

        SelectableProfiles.Add(new FilterProfile(false)
        {
            Name = "No profile"
        });

        AllProfiles.Add(DefaultProfile);
    }

    /// <summary>
    /// Gets or sets whether the default filter is enabled.
    /// </summary>
    public bool DefaultFilterEnabled
    {
        get => _defaultFilterEnabled;
        set
        {
            if (!_defaultFilterEnabled && value)
            {
                SelectableProfiles.Insert(0, DefaultProfile);
            }
            else if (_defaultFilterEnabled && !value)
            {
                SelectableProfiles.Remove(DefaultProfile);
            }

            _defaultFilterEnabled = value;
        }
    }

    /// <summary>
    /// Gets or sets the default filter.
    /// </summary>
    public FilterProfile DefaultProfile { get; }

    /// <summary>
    /// Gets or sets the collection of profiles.
    /// </summary>
    public ObservableCollection<FilterProfile> SelectableProfiles { get; }

    /// <summary>
    /// Gets or sets the collection of profiles.
    /// </summary>
    public ObservableCollection<FilterProfile> AllProfiles { get; }

    /// <summary>
    /// Add the given profile.
    /// </summary>
    /// <param name="profile">The profile to add.</param>
    public void AddProfile(FilterProfile profile)
    {
        SelectableProfiles.Insert(SelectableProfiles.Count - 1, profile);
        AllProfiles.Add(profile);
    }

    /// <summary>
    /// Remove the given filter.
    /// </summary>
    /// <param name="profile">The profile to remove.</param>
    public void RemoveProfile(FilterProfile profile)
    {
        SelectableProfiles.Remove(profile);
        AllProfiles.Remove(profile);
    }
}