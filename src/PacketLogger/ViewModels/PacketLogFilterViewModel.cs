//
//  PacketLogFilterViewModel.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia;
using Avalonia.Input.Platform;
using DynamicData;
using Microsoft.VisualBasic;
using PacketLogger.Models.Filters;
using ReactiveUI;

namespace PacketLogger.ViewModels;

/// <inheritdoc />
public class PacketLogFilterViewModel : ViewModelBase, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PacketLogFilterViewModel"/> class.
    /// </summary>
    public PacketLogFilterViewModel()
    {
        Filters = new ObservableCollection<FilterCreator.FilterData>();
        RemoveCurrent = ReactiveCommand.Create
        (
            () =>
            {
                var selected = SelectedFilter;
                if (selected is not null)
                {
                    var selectedIndex = Filters.IndexOf(selected);
                    SelectedFilter = Filters.Count > selectedIndex + 1 ? Filters[selectedIndex + 1] : null;
                    if (SelectedFilter is null && selectedIndex > 0)
                    {
                        SelectedFilter = Filters[selectedIndex - 1];
                    }

                    Filters.Remove(selected);
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
                    Filters.Add(newFilter);
                    NewFilter = string.Empty;
                    SelectedFilter = newFilter;
                }
            }
        );
    }

    /// <summary>
    /// Gets or sets whether the filters should whitelist or blacklist.
    /// </summary>
    public bool Whitelist { get; set; }

    /// <summary>
    /// Gets or sets the currently selected filter.
    /// </summary>
    public FilterCreator.FilterData? SelectedFilter { get; set; }

    /// <summary>
    /// Gets or sets the filters list.
    /// </summary>
    public ObservableCollection<FilterCreator.FilterData> Filters { get; }

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

    /// <inheritdoc />
    public void Dispose()
    {
        RemoveCurrent.Dispose();
        AddNew.Dispose();
    }
}