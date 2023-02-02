//
//  ViewLocator.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PacketLogger.ViewModels;

namespace PacketLogger;

/// <inheritdoc />
public class ViewLocator : IDataTemplate
{
    /// <inheritdoc />
    public IControl Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "View not selected." };
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <inheritdoc />
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}