//
//  FilterChooseViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using DynamicData.Binding;
using PacketLogger.Models.Filters;
using ReactiveUI;

namespace PacketLogger.ViewModels.Filters;

/// <summary>
/// A view model for filter choose view.
/// </summary>
public class FilterChooseViewModel : ViewModelBase, IDisposable
{
    private FilterProfile _currentProfile = null!;
    private IDisposable? _cleanUp;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterChooseViewModel"/> class.
    /// </summary>
    /// <param name="currentProfile">The current filter profile.</param>
    public FilterChooseViewModel(FilterProfile currentProfile)
    {
        RecvFilterSelected = true;
        CurrentProfile = currentProfile;
        CurrentFilter = CreateSendRecvFilter();
    }

    /// <summary>
    /// Gets the current profile.
    /// </summary>
    public FilterProfile CurrentProfile
    {
        get => _currentProfile;
        set
        {
            _currentProfile = value;
            RecvEntryViewModel = new FilterEntryViewModel(_currentProfile.RecvFilterEntry);
            SendEntryViewModel = new FilterEntryViewModel(_currentProfile.SendFilterEntry);

            var recvWhenAny = _currentProfile.RecvFilterEntry.WhenAnyPropertyChanged("Active", "Whitelist")
                .Subscribe((e) => OnChange());

            var sendWhenAny = _currentProfile.SendFilterEntry.WhenAnyPropertyChanged("Active", "Whitelist")
                .Subscribe((e) => OnChange());

            var recvFilters = _currentProfile.RecvFilterEntry.Filters.ObserveCollectionChanges()
                .Subscribe((e) => OnChange());

            var sendFilters = _currentProfile.SendFilterEntry.Filters.ObserveCollectionChanges()
                .Subscribe((e) => OnChange());

            _cleanUp?.Dispose();
            _cleanUp = new CompositeDisposable(recvWhenAny, sendWhenAny, recvFilters, sendFilters);
            OnChange();
        }
    }

    /// <summary>
    /// Gets the current recv entry view model.
    /// </summary>
    public FilterEntryViewModel RecvEntryViewModel { get; private set; } = null!;

    /// <summary>
    /// Gets the current send entry view model.
    /// </summary>
    public FilterEntryViewModel SendEntryViewModel { get; private set; } = null!;

    /// <summary>
    /// Gets whether the send filter is currently selected.
    /// </summary>
    public bool SendFilterSelected { get; set; }

    /// <summary>
    /// Gets whether the recv filter is currently selected.
    /// </summary>
    public bool RecvFilterSelected { get; set; }

    /// <summary>
    /// Gets the currently selected filter.
    /// </summary>
    public IFilter CurrentFilter { get; private set; }

    private void OnChange()
    {
        CurrentFilter = CreateSendRecvFilter();
    }

    /// <summary>
    /// Create a filter out of the chosen filters.
    /// </summary>
    /// <returns>The created filter.</returns>
    public IFilter CreateSendRecvFilter()
    {
        IFilter recvFilter = CreateCompound(RecvEntryViewModel.Entry);
        IFilter sendFilter = CreateCompound(SendEntryViewModel.Entry);

        return new SendRecvFilter(sendFilter, recvFilter);
    }

    private IFilter CreateCompound(FilterProfileEntry filterEntry)
    {
        if (!filterEntry.Active)
        {
            return new CompoundFilter(true);
        }

        List<IFilter> filters = new List<IFilter>();

        foreach (var filter in filterEntry.Filters)
        {
            filters.Add(FilterCreator.BuildFilter(filter.Type, filter.Value));
        }

        return new CompoundFilter(!filterEntry.Whitelist, filters.ToArray());
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cleanUp?.Dispose();
    }
}