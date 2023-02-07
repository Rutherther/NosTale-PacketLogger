//
//  FilterConfigViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PacketLogger.Models.Filters;

namespace PacketLogger.ViewModels.Filters;

/// <summary>
/// A view model for FilterConfigView.
/// </summary>
public class FilterConfigViewModel : ViewModelBase
{
    private readonly FilterProfile _filterProfile;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterConfigViewModel"/> class.
    /// </summary>
    /// <param name="filterProfile">The filter profile to show and configure.</param>
    public FilterConfigViewModel(FilterProfile filterProfile)
    {
        _filterProfile = filterProfile;
        RecvEntryViewModel = new FilterEntryViewModel(filterProfile.RecvFilterEntry);
        SendEntryViewModel = new FilterEntryViewModel(filterProfile.SendFilterEntry);
    }

    /// <summary>
    /// Gets the filter profile.
    /// </summary>
    public FilterProfile Profile => _filterProfile;

    /// <summary>
    /// Gets the recv entry view model.
    /// </summary>
    public FilterEntryViewModel RecvEntryViewModel { get; }

    /// <summary>
    /// Gets the send entry view model.
    /// </summary>
    public FilterEntryViewModel SendEntryViewModel { get; }
}