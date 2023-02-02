//
//  DockFactory.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using NosSmooth.Comms.Local;
using NosSmooth.Core.Stateful;
using PacketLogger.Models;
using ReactiveUI;

namespace PacketLogger.ViewModels;

/// <summary>
/// A factory for root dock.
/// </summary>
public class DockFactory : Factory, IDisposable
{
    private readonly StatefulRepository _repository;
    private readonly NostaleProcesses _processes = new();
    private readonly CommsInjector _injector;

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
                var document = new PacketLogDocumentViewModel(_injector, _repository, _processes)
                    { Id = $"New tab {index}", Title = $"New tab {index}" };

                AddDockable(documentDock, document);
                SetActiveDockable(document);
                SetFocusedDockable(documentDock, document);
            }
        );
        return documentDock;
    }

    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockFactory"/> class.
    /// </summary>
    /// <param name="injector">The communications injector.</param>
    /// <param name="repository">The repository.</param>
    public DockFactory(CommsInjector injector, StatefulRepository repository)
    {
        _repository = repository;
        _injector = injector;
    }

    /// <inheritdoc />
    public override IRootDock CreateLayout()
    {
        var initialTab = new PacketLogDocumentViewModel(_injector, _repository, _processes)
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