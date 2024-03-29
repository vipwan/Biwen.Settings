﻿using Biwen.Settings.Infrastructure.TypeFinder;

namespace Biwen.Settings.Infrastructure
{

    /// <summary>
    /// Assembly Helper
    /// </summary>
    public static class Assemblies
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
                    var ass = Assembly.GetEntryAssembly()!.GetReferencedAssemblies();
                    foreach (var @as in ass)
                    {
                        Assembly.Load(@as);
                    }
                    _allRequiredAssembliesFound = true;
                }
                return _allRequiredAssemblies ??=
                    AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !EscapeAssemblies
                    .Any(a => x.FullName!.StartsWith(a)))
                    .ToArray();
            }
        }

        /// <summary>
        /// Extension
        /// </summary>
        public static IInAssemblyFinder InAllRequiredAssemblies => FindTypes.InAssemblies(AllRequiredAssemblies);

    }
}