//
//  DockFactory.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using NosSmooth.Comms.Local;
using NosSmooth.Core.Stateful;
using PacketLogger.Models;
using PacketLogger.Models.Packets;
using PacketLogger.Views;
using ReactiveUI;
using HostWindow = PacketLogger.Views.HostWindow;

namespace PacketLogger.ViewModels;

/// <summary>
/// A factory for root dock.
/// </summary>
public class DockFactory : Factory, IDisposable
{
    private readonly StatefulRepository _repository;
    private readonly ObservableCollection<IPacketProvider> _providers;
    private readonly NostaleProcesses _processes;
    private readonly CommsInjector _injector;

    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockFactory"/> class.
    /// </summary>
    /// <param name="providers">The providers.</param>
    /// <param name="processes">The nostale processes.</param>
    /// <param name="injector">The communications injector.</param>
    /// <param name="repository">The repository.</param>
    public DockFactory
    (
        ObservableCollection<IPacketProvider> providers,
        NostaleProcesses processes,
        CommsInjector injector,
        StatefulRepository repository
    )
    {
        _providers = providers;
        _processes = processes;
        _repository = repository;
        _injector = injector;
    }

    /// <summary>
    /// Document loaded event.
    /// </summary>
    public event Action<DocumentViewModel>? DocumentLoaded;

    /// <summary>
    /// Document closed event.
    /// </summary>
    public event Action<DocumentViewModel>? DocumentClosed;

    private void OnDocumentLoaded(DocumentViewModel documentViewModel)
    {
        DocumentLoaded?.Invoke(documentViewModel);
    }

    private void OnDocumentClosed(DocumentViewModel documentViewModel)
    {
        DocumentClosed?.Invoke(documentViewModel);
    }

    /// <summary>
    /// Gets the document dock.
    /// </summary>
    public IDocumentDock DocumentDock
        => _documentDock ?? throw new InvalidOperationException("Dock factory is not initialized");

    /// <summary>
    /// Creaate and load a document.
    /// </summary>
    /// <param name="load">The function to use to load the document.</param>
    public void CreateLoadedDocument(Func<DocumentViewModel, IObservable<Unit>> load)
    {
        if (_documentDock is null)
        {
            return;
        }

        var document = new DocumentViewModel
            (
                _injector,
                _repository,
                _providers,
                _processes,
                OnDocumentLoaded,
                OnDocumentClosed
            )
            { Id = $"New tab", Title = $"New tab" };

        var observable = load(document);
        observable.Subscribe
        (
            x =>
            {
                if (!document.Loaded)
                {
                    return;
                }

                AddDockable(_documentDock, document);
                SetActiveDockable(document);
                SetFocusedDockable(_documentDock, document);
            }
        );
    }

    /// <inheritdoc />
    public override IDocumentDock CreateDocumentDock()
    {
        var documentDock = new DocumentDock();
        documentDock.CreateDocument = ReactiveCommand.Create
        (
            () =>
            {
                if (!documentDock.CanCreateDocument)
                {
                    return;
                }

                var index = documentDock.VisibleDockables?.Count + 1;
                var document = new DocumentViewModel
                    (
                        _injector,
                        _repository,
                        _providers,
                        _processes,
                        OnDocumentLoaded,
                        OnDocumentClosed
                    )
                    { Id = $"New tab {index}", Title = $"New tab {index}" };

                AddDockable(documentDock, document);
                SetActiveDockable(document);
                SetFocusedDockable(documentDock, document);
            }
        );
        return documentDock;
    }

    /// <inheritdoc />
    public override IRootDock CreateLayout()
    {
        var initialTab = new DocumentViewModel
            (
                _injector,
                _repository,
                _providers,
                _processes,
                OnDocumentLoaded,
                OnDocumentClosed
            )
            { Id = $"New tab", Title = $"New tab" };
        var documentDock = CreateDocumentDock();
        documentDock.IsCollapsable = false;
        documentDock.ActiveDockable = initialTab;
        documentDock.VisibleDockables = CreateList<IDockable>(initialTab);
        documentDock.CanCreateDocument = true;

        var rootDock = CreateRootDock();

        rootDock.IsCollapsable = false;
        rootDock.ActiveDockable = documentDock;
        rootDock.DefaultDockable = documentDock;
        rootDock.VisibleDockables = CreateList<IDockable>(documentDock);

        _documentDock = documentDock;
        _rootDock = rootDock;

        return rootDock;
    }

    /// <inheritdoc />
    public override void InitLayout(IDockable layout)
    {
        ContextLocator = new Dictionary<string, Func<object>>
        {
            ["Home"] = () => layout
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>
        {
            ["Root"] = () => _rootDock,
            ["Documents"] = () => _documentDock
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _processes.Dispose();
    }
}