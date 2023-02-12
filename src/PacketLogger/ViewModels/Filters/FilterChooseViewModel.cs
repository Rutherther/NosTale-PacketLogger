//
//  FilterChooseViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reflection.Metadata.Ecma335;
using Avalonia.Controls.Notifications;
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
    private FilterProfile _currentRealProfile = null!;
    private IDisposable? _cleanUp;
    private FilterProfile _noProfile;
    private FilterProfile _noRealProfile;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterChooseViewModel"/> class.
    /// </summary>
    /// <param name="currentProfile">The current filter profile.</param>
    /// <param name="noProfile">The real no profile.</param>
    public FilterChooseViewModel(FilterProfile currentProfile, FilterProfile noProfile)
    {
        _noProfile = new FilterProfile(false)
        {
            Name = "No profile"
        };
        _noRealProfile = noProfile;
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
            if (value is null)
            {
                return;
            }

            var setCurrentProfile = value;
            if (value.Name == "No profile")
            {
                if (value != _noProfile)
                {
                    value = _noProfile;
                }
                else
                {
                    setCurrentProfile = _noRealProfile;
                }

                CopyCurrentToNoProfile(_currentProfile);
            }

            var cleanUp = new CompositeDisposable();
            if (RecvEntryViewModel is not null)
            {
                cleanUp = new CompositeDisposable(RecvEntryViewModel, SendEntryViewModel);
            }

            var lastProfile = value;
            if (value != _noProfile)
            {
                RecvEntryViewModel = new FilterEntryViewModel
                (
                    value.RecvFilterEntry,
                    (data) =>
                    {
                        CurrentProfile = _noProfile;
                        _noProfile.RecvFilterEntry.Filters.Add(data);
                    },
                    (data) =>
                    {
                        CurrentProfile = _noProfile;
                        _noProfile.RecvFilterEntry.Filters.Remove(data);
                    },
                    (data) =>
                    {
                        CurrentProfile = _noProfile;
                        _noProfile.RecvFilterEntry.Active = data.Active;
                        _noProfile.RecvFilterEntry.Whitelist = data.Whitelist;
                    }
                );
                SendEntryViewModel = new FilterEntryViewModel
                (
                    value.SendFilterEntry,
                    (data) =>
                    {
                        CurrentProfile = _noProfile;
                        _noProfile.SendFilterEntry.Filters.Add(data);
                    },
                    (data) =>
                    {
                        CurrentProfile = _noProfile;
                        _noProfile.SendFilterEntry.Filters.Remove(data);
                    },
                    (data) =>
                    {
                        CurrentProfile = _noProfile;
                        _noProfile.SendFilterEntry.Active = data.Active;
                        _noProfile.SendFilterEntry.Whitelist = data.Whitelist;
                    }
                );
            }
            else
            {
                RecvEntryViewModel = new FilterEntryViewModel(value.RecvFilterEntry);
                SendEntryViewModel = new FilterEntryViewModel(value.SendFilterEntry);
            }

            _currentRealProfile = value;
            _currentProfile = setCurrentProfile;

            var recvWhenAny = _currentRealProfile.RecvFilterEntry.WhenAnyPropertyChanged("Active", "Whitelist")
                .Subscribe((e) => OnChange());

            var sendWhenAny = _currentRealProfile.SendFilterEntry.WhenAnyPropertyChanged("Active", "Whitelist")
                .Subscribe((e) => OnChange());

            var recvFilters = _currentRealProfile.RecvFilterEntry.Filters.ObserveCollectionChanges()
                .Subscribe((e) => OnChange());

            var sendFilters = _currentRealProfile.SendFilterEntry.Filters.ObserveCollectionChanges()
                .Subscribe((e) => OnChange());

            _cleanUp?.Dispose();
            _cleanUp = new CompositeDisposable(recvWhenAny, sendWhenAny, recvFilters, sendFilters, cleanUp);
            OnChange();
        }
    }

    private void CopyCurrentToNoProfile(FilterProfile? lastProfile)
    {
        if (lastProfile is null)
        {
            return;
        }

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
        IFilter recvFilter = CreateCompound(_currentRealProfile.RecvFilterEntry);
        IFilter sendFilter = CreateCompound(_currentRealProfile.SendFilterEntry);

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