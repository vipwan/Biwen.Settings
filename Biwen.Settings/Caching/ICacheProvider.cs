﻿// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:26:30 ICacheProvider.cs

namespace Biwen.Settings.Caching;

/// <summary>
/// 缓存提供者,应用可支持多种缓存,如内存缓存,Redis缓存等 实现分布式缓存
/// </summary>
public interface ICacheProvider
{

    /// <summary>
    /// 创建或获取缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="cacheTime">缓存周期秒:seconds </param>
    /// <returns></returns>
    Task<T?> GetOrCreateAsync<T>(string key, Func<T?> factory, int cacheTime = 24 * 60 * 60) where T : ISetting;

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="key"></param>
    Task RemoveAsync(string key);

    /// <summary>
    /// 移除所有缓存
    /// </summary>
    /// <returns></returns>
    Task RemoveAllAsync();
}