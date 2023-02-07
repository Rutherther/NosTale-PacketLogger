//
//  FilterProfile.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;

namespace PacketLogger.Models.Filters;

/// <summary>
/// A filter profile.
/// </summary>
public class FilterProfile : ObservableObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterProfile"/> class.
    /// </summary>
    /// <param name="isDefault">Whether this profile is a default profile.</param>
    public FilterProfile(bool isDefault)
    {
        IsDefault = isDefault;
        Name = "New filter";
        RecvFilterEntry = new FilterProfileEntry();
        SendFilterEntry = new FilterProfileEntry();
    }

    /// <summary>
    /// Gets whether this profile is a default profile.
    /// </summary>
    public bool IsDefault { get; }

    /// <summary>
    /// Gets or sets the name of the filter profile.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the receive filter entry.
    /// </summary>
    public FilterProfileEntry RecvFilterEntry { get; }

    /// <summary>
    /// Gets or sets the send filter entry.
    /// </summary>
    public FilterProfileEntry SendFilterEntry { get; }
}