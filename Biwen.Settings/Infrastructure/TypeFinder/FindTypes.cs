// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:28:06 FindTypes.cs

namespace Biwen.Settings.Infrastructure.TypeFinder;

public static class FindTypes
{
    public static IInAssemblyFinder InAssembly(Assembly assembly) => new InAssemblyFinder([assembly]);

    public static IInAssemblyFinder InAssemblies(params Assembly[] assemblies) => new InAssemblyFinder(assemblies);

    public static IInAssemblyFinder InCurrentAssembly => InAssembly(Assembly.GetCallingAssembly());

    public static IInAssemblyFinder InAllAssemblies => InAssemblies(AppDomain.CurrentDomain.GetAssemblies());
}
