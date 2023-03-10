//
//  DocumentView.axaml.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace PacketLogger.Views;

/// <summary>
/// A view of document tab.
/// </summary>
[DoNotNotify]
public partial class DocumentView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentView"/> class.
    /// </summary>
    public DocumentView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}