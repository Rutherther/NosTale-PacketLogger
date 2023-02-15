//
//  NumberedTitleGenerator.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;

namespace PacketLogger.Models.Titles;

/// <summary>
/// A generator and manager of document titles.
/// </summary>
public class NumberedTitleGenerator
{
    private readonly object _lock = new object();
    private readonly ConcurrentDictionary<string, List<Title>> _titles;

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberedTitleGenerator"/> class.
    /// </summary>
    public NumberedTitleGenerator()
    {
        _titles = new ConcurrentDictionary<string, List<Title>>();
    }

    /// <summary>
    /// Add title with given information.
    /// </summary>
    /// <param name="setDocumentTitle">The function used for setting new document title.</param>
    /// <param name="titleChanged">The observable observing changes to the default, unnumbered title.</param>
    /// <param name="initialTitle">The current initial title.</param>
    /// <returns>The handle, title will be removed upon disposal.</returns>
    public TitleHandle AddTitle
    (
        Action<string> setDocumentTitle,
        IObservable<string> titleChanged,
        string initialTitle
    )
    {
        var title = new Title
        (
            setDocumentTitle,
            initialTitle
        );

        title.TitleChanged = titleChanged.Subscribe
        (
            newTitle => HandleTitleChange(title, newTitle)
        );
        HandleTitleChange(title, title.CurrentTitle);

        return new TitleHandle(this, title);
    }

    private void RemoveTitle(Title title)
    {
        _titles.AddOrUpdate
        (
            title.CurrentTitle,
            _ => new List<Title>(),
            (_, u) =>
            {
                u.Remove(title);
                return u;
            }
        );
        UpdateNumbers(title.CurrentTitle);
    }

    private void HandleTitleChange(Title title, string newTitle)
    {
        lock (_lock)
        {
            _titles.AddOrUpdate
            (
                title.CurrentTitle,
                _ => new List<Title>(),
                (_, u) =>
                {
                    u.Remove(title);
                    return u;
                }
            );
            UpdateNumbers(title.CurrentTitle);

            title.CurrentTitle = newTitle;
            _titles.TryAdd(newTitle, new List<Title>());
            _titles.AddOrUpdate
            (
                newTitle,
                _ => new List<Title>(),
                (_, u) =>
                {
                    u.Add(title);
                    return u;
                }
            );
            UpdateNumbers(title.CurrentTitle);
        }
    }

    private void UpdateNumbers(string title)
    {
        if (_titles.TryGetValue(title, out var titles))
        {
            if (titles.Count == 1)
            {
                titles[0].CurrentNumber = 0;
                titles[0].SetDocumentTitle(titles[0].CurrentTitle);
            }
            else if (titles.Count > 1)
            {
                titles[0].CurrentNumber = null;
                titles[0].SetDocumentTitle(titles[0].CurrentTitle);

                for (int i = 1; i < titles.Count; i++)
                {
                    titles[i].CurrentNumber = i;
                    titles[i].SetDocumentTitle($"{titles[i].CurrentTitle} ({i})");
                }
            }
        }
    }

    /// <summary>
    /// A store of a title.
    /// </summary>
    public class TitleHandle : IDisposable
    {
        private readonly NumberedTitleGenerator _titleGenerator;
        private readonly Title _title;

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleHandle"/> class.
        /// </summary>
        /// <param name="titleGenerator">The title generator.</param>
        /// <param name="title">The title.</param>
        public TitleHandle(NumberedTitleGenerator titleGenerator, Title title)
        {
            _titleGenerator = titleGenerator;
            _title = title;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _title.TitleChanged.Dispose();
            _titleGenerator.RemoveTitle(_title);
        }
    }

    /// <summary>
    /// A title.
    /// </summary>
    /// <param name="SetDocumentTitle">The function used for setting the title of document.</param>
    /// <param name="CurrentTitle">The current title.</param>
    public record Title(Action<string> SetDocumentTitle, string CurrentTitle)
    {
        /// <summary>
        /// Gets or sets the current suffix.
        /// </summary>
        public int? CurrentNumber { get; set; }

        /// <summary>
        /// Gets or sets the disposable title changed observer.
        /// </summary>
        public IDisposable TitleChanged { get; set; } = null!;

        /// <summary>
        /// Gets or sets the current title.
        /// </summary>
        public string CurrentTitle { get; set; } = CurrentTitle;
    }
}