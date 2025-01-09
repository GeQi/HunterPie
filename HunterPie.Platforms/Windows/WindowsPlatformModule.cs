﻿using HunterPie.DI;
using HunterPie.DI.Module;
using HunterPie.Platforms.Windows.Process;
using HunterPie.Platforms.Windows.Registry;

namespace HunterPie.Platforms.Windows;

internal class WindowsPlatformModule : IDependencyModule
{
    public void Register(IDependencyRegistry registry)
    {
        registry
            .WithSingle<WindowsProcessWatcher>()
            .WithSingle<WindowsRegistry>();
    }
}