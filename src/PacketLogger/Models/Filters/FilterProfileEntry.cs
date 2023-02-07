//
//  FilterProfileEntry.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PacketLogger.Models.Filters;

/// <summary>
/// Receive of send entry of a <see cref="FilterProfile"/>.
/// </summary>
public class FilterProfileEntry : ObservableObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterProfileEntry"/> class.
    /// </summary>
    public FilterProfileEntry()
    {
        Active = true;
        Filters = new ObservableCollection<FilterCreator.FilterData>();
    }

    /// <summary>
    /// Gets or sets whether the filter is active.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets whether the filters should whitelist or blacklist.
    /// </summary>
    public bool Whitelist { get; set; }

    /// <summary>
    /// Gets or sets the filters list.
    /// </summary>
    public ObservableCollection<FilterCreator.FilterData> Filters { get; }
}