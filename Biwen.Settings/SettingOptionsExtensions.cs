﻿// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:30:57 SettingOptionsExtensions.cs

using Biwen.Settings.Caching;
using Biwen.Settings.SettingManagers.EFCore;
using Biwen.Settings.SettingManagers.JsonStore;

namespace Biwen.Settings
{
    public static class SettingOptionsExtensions
    {
        /// <summary>
        /// 使用内存缓存
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static SettingOptions UseCacheOfMemory(this SettingOptions options)
        {
            options.UseCache<MemoryCacheProvider>();
            return options;
        }

        /// <summary>
        /// Default不使用缓存
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static SettingOptions UseCacheOfNull(this SettingOptions options)
        {
            options.UseCache<NullCacheProvider>();
            return options;
        }

        /// <summary>
        /// 使用EntityFmeworkCore持久化配置项
        /// </summary>
        /// <param name="options"></param>
        /// <param name="storePptions"></param>
        /// <returns></returns>
        public static SettingOptions UseStoreOfEFCore(this SettingOptions options, Action<EFCoreStoreOptions>? storeOptions = null)
        {
            options.UseSettingManager<EntityFrameworkCoreSettingManager, Action<EFCoreStoreOptions>?>(storeOptions);
            return options;
        }

        /// <summary>
        /// 使用JsonStore持久化配置项
        /// </summary>
        /// <param name="options"></param>
        /// <param name="storeOptions"></param>
        /// <returns></returns>
        public static SettingOptions UseStoreOfJsonFile(this SettingOptions options, Action<JsonStoreOptions>? storeOptions = null)
        {
            options.UseSettingManager<JsonStoreSettingManager, Action<JsonStoreOptions>?>(storeOptions);
            return options;
        }
    }

}