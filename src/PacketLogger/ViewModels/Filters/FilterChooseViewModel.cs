//
//  FilterChooseViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using DynamicData;
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
    private FilterProfile _noProfile;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterChooseViewModel"/> class.
    /// </summary>
    /// <param name="currentProfile">The current filter profile.</param>
    public FilterChooseViewModel(FilterProfile currentProfile)
    {
        RecvFilterSelected = true;
        CurrentProfile = currentProfile;
        CurrentFilter = CreateSendRecvFilter();
        _noProfile = new FilterProfile(false)
        {
            Name = "No profile"
        };
    }

    /// <summary>
    /// Gets the current profile.
    /// </summary>
    public FilterProfile CurrentProfile
    {
        get => _currentProfile;
        set
        {
            if (value is null)
            {
                return;
            }

            if (value.Name == "No profile" && value != _noProfile)
            {
                CurrentProfile = _noProfile;
                return;
            }

            var lastProfile = value;
            if (value != _noProfile)
            {
                RecvEntryViewModel = new FilterEntryViewModel
                (
                    value.RecvFilterEntry,
                    (data) =>
                    {
                        CopyCurrentToNoProfile(lastProfile);
                        _noProfile.RecvFilterEntry.Filters.Add(data);
                        CurrentProfile = _noProfile;
                    },
                    (data) =>
                    {
                        CopyCurrentToNoProfile(lastProfile);
                        _noProfile.RecvFilterEntry.Filters.Remove(data);
                        CurrentProfile = _noProfile;
                    }
                );
                SendEntryViewModel = new FilterEntryViewModel
                (
                    value.SendFilterEntry,
                    (data) =>
                    {
                        CopyCurrentToNoProfile(lastProfile);
                        _noProfile.SendFilterEntry.Filters.Add(data);
                        CurrentProfile = _noProfile;
                    },
                    (data) =>
                    {
                        CopyCurrentToNoProfile(lastProfile);
                        _noProfile.SendFilterEntry.Filters.Remove(data);
                        CurrentProfile = _noProfile;
                    }
                );
            }
            else
            {
                RecvEntryViewModel = new FilterEntryViewModel(value.RecvFilterEntry, null, null);
                SendEntryViewModel = new FilterEntryViewModel(value.SendFilterEntry, null, null);
            }
            _currentProfile = value;

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

    private void CopyCurrentToNoProfile(FilterProfile lastProfile)
    {
        _noProfile.RecvFilterEntry.Filters.Clear();
        _noProfile.RecvFilterEntry.Filters.AddRange(lastProfile.RecvFilterEntry.Filters);

        _noProfile.SendFilterEntry.Filters.Clear();
        _noProfile.SendFilterEntry.Filters.AddRange(lastProfile.SendFilterEntry.Filters);

        _noProfile.RecvFilterEntry.Active = lastProfile.RecvFilterEntry.Active;
        _noProfile.RecvFilterEntry.Whitelist = lastProfile.RecvFilterEntry.Whitelist;
        _noProfile.SendFilterEntry.Active = lastProfile.SendFilterEntry.Active;
        _noProfile.SendFilterEntry.Whitelist = lastProfile.SendFilterEntry.Whitelist;
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