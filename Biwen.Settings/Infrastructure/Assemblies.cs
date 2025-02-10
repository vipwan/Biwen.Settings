// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:27:59 Assemblies.cs

using Biwen.Settings.Infrastructure.TypeFinder;

namespace Biwen.Settings.Infrastructure;

/// <summary>
/// Assembly Helper
/// </summary>
internal static class Assemblies
{
    /// <summary>
    /// 排除的程序集
    /// </summary>
    private static readonly string[] EscapeAssemblies =
    [
        "netstandard",
        "Microsoft",
        "Mono",
        "Scrutor",//Scrutor
        "Humanizer",
        "SQLitePCLRaw",//Sqlite
        "System",
        "Newtonsoft",
        "Swashbuckle",
        "AutoMapper",
        "FluentValidation",
        "Biwen.AutoClassGen" //Biwen.AutoClassGen
    ];

    private static Assembly[] _allRequiredAssemblies = null!;
    private static bool _allRequiredAssembliesFound = false;
    /// <summary>
    /// 排除公共程序集后的所有程序集
    /// </summary>
    public static Assembly[] AllRequiredAssemblies
    {
        get
        {
            if (!_allRequiredAssembliesFound)
            {
                // 装载所有引用的程序集
                //var ass = Assembly.GetEntryAssembly()!.GetReferencedAssemblies();
                //foreach (var @as in ass)
                //{
                //    Assembly.Load(@as);
                //}

                var dlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
                //排除程序集名称以EscapeAssemblies集合开头的程序集:
                var needDlls = new List<string>();
                foreach (var dll in dlls)
                {
                    var assemblyName = Path.GetFileName(dll);
                    var flag = false;
                    foreach (var escape in EscapeAssemblies)
                    {
                        if (assemblyName.StartsWith(escape))
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                        needDlls.Add(dll);
                }

                var assemblies = needDlls.Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)));
                _allRequiredAssemblies ??= [.. assemblies];
                _allRequiredAssembliesFound = true;
            }
            return _allRequiredAssemblies;
        }
    }

    /// <summary>
    /// Extension
    /// </summary>
    public static IInAssemblyFinder InAllRequiredAssemblies => FindTypes.InAssemblies(AllRequiredAssemblies);

}