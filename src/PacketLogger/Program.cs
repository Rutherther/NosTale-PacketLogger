//
//  Program.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;

namespace PacketLogger;

/// <summary>
/// Application entrypoint class.
/// </summary>
public class Program
{
    /// <summary>
    /// Application entrypoint.
    /// </summary>
    /// <param name="args">The CLI arguments.</param>
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Build the Avalonia app.
    /// </summary>
    /// <returns>The app builder.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI()
            .WithIcons(c => c.Register<MaterialDesignIconProvider>())
            .With(new FontManagerOptions() { DefaultFamilyName = "Arial" });
}