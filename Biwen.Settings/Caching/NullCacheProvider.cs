// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:26:56 NullCacheProvider.cs

namespace Biwen.Settings.Caching;

/// <summary>
/// 默认的缓存提供者,不做任何缓存
/// </summary>
internal sealed class NullCacheProvider : ICacheProvider
{
    public Task<T?> GetOrCreateAsync<T>(string key, Func<T?> factory, int cacheTime = 86400) where T : ISetting
    {
        return Task.FromResult(factory());
    }

    public Task RemoveAllAsync()
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        return Task.CompletedTask;
    }
}