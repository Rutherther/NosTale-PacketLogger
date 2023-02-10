//
//  FilterEntryViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reactive;
using PacketLogger.Models.Filters;
using ReactiveUI;

namespace PacketLogger.ViewModels.Filters;

/// <summary>
/// A view model for FilterEntryView.
/// </summary>
public class FilterEntryViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterEntryViewModel"/> class.
    /// </summary>
    /// <param name="entry">The profile entry.</param>
    /// <param name="addNew">The action called upon adding new.</param>
    /// <param name="remove">The action called upon removing the given filter data.</param>
    public FilterEntryViewModel
        (FilterProfileEntry entry, Action<FilterCreator.FilterData>? addNew, Action<FilterCreator.FilterData>? remove)
    {
        NewFilterType = FilterCreator.FilterType.PacketHeader;
        Entry = entry;
        RemoveCurrent = ReactiveCommand.Create
        (
            () =>
            {
                var selected = SelectedFilter;
                if (selected is not null)
                {
                    if (remove is not null)
                    {
                        SelectedFilter = null;
                        remove(selected);
                    }
                    else
                    {
                        var selectedIndex = Entry.Filters.IndexOf(selected);
                        SelectedFilter = Entry.Filters.Count > selectedIndex + 1
                            ? Entry.Filters[selectedIndex + 1]
                            : null;
                        if (SelectedFilter is null && selectedIndex > 0)
                        {
                            SelectedFilter = Entry.Filters[selectedIndex - 1];
                        }

                        Entry.Filters.Remove(selected);
                    }
                }
            }
        );

        AddNew = ReactiveCommand.Create
        (
            () =>
            {
                if (!string.IsNullOrEmpty(NewFilter))
                {
                    var newFilter = new FilterCreator.FilterData(NewFilterType, NewFilter);
                    if (addNew is not null)
                    {
                        addNew(newFilter);
                    }
                    else
                    {
                        Entry.Filters.Add(newFilter);
                    }

                    NewFilter = string.Empty;
                    SelectedFilter = newFilter;
                }
            }
        );
    }

    /// <summary>
    /// Gets the filter profile entry associated with this view model.
    /// </summary>
    public FilterProfileEntry Entry { get; }

    /// <summary>
    /// Gets or sets the currently selected filter.
    /// </summary>
    public FilterCreator.FilterData? SelectedFilter { get; set; }

    /// <summary>
    /// Gets or sets the type of the new filter to add.
    /// </summary>
    public FilterCreator.FilterType NewFilterType { get; set; }

    /// <summary>
    /// Gets or sets the string of the new filter to add.
    /// </summary>
    public string NewFilter { get; set; } = string.Empty;

    /// <summary>
    /// Gets the command to remove the currently selected filter.
    /// </summary>
    public ReactiveCommand<Unit, Unit> RemoveCurrent { get; }

    /// <summary>
    /// Gets the command to add new filter.
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddNew { get; }
}