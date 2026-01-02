// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:26:42 MemoryCacheProvider.cs

using Microsoft.Extensions.Caching.Memory;

namespace Biwen.Settings.Caching;

/// <summary>
/// 默认的缓存提供者,使用内存缓存
/// </summary>
public sealed class MemoryCacheProvider(IMemoryCache cache) : ICacheProvider
{
    private readonly IMemoryCache _cache = cache;

    private static readonly HashSet<string> Keys = [];

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<T?> factory, int cacheTime = 86400) where T : ISetting
    {
        Keys.Add(key);

        return await _cache.GetOrCreateAsync<T>(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheTime);
            var @default = factory();
            return Task.FromResult(@default!);
        });
    }

    public Task RemoveAllAsync()
    {
        foreach (var key in Keys)
        {
            _cache.Remove(key);
        }
        Keys.Clear();
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        //删除Key
        Keys.Remove(key);

        _cache.Remove(key);
        return Task.CompletedTask;
    }

}