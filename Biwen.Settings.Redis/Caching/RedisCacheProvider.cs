// Licensed to the Biwen.Settings.Redis under one or more agreements.
// The Biwen.Settings.Redis licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings.Redis Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-12-25 14:08:30 RedisCacheProvider.cs

using Biwen.Settings.Caching;
using CSRedis;
using Microsoft.Extensions.Logging;

namespace Biwen.Settings.Redis.Caching;

public class RedisCacheProvider(CSRedisClient client, ILogger<RedisCacheProvider> logger) : ICacheProvider
{
    private const string SettingKeyFormat = "__BiwenSetting__CsRedis_";

    private static readonly HashSet<string> SettingKeys = [];

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<T?> factory, int cacheTime = 86400) where T : ISetting
    {
        var cacheKey = $"{SettingKeyFormat}{key}";

        //add to cache key list
        SettingKeys.Add(cacheKey);

        logger.LogDebug("GetOrCreateAsync: {key}", key);

        if (client.Exists(cacheKey))
        {
            return await client.GetAsync<T>(cacheKey);
        }
        var value = factory();
        await client.SetAsync(cacheKey, factory(), cacheTime);

        return value;
    }

    public async Task RemoveAllAsync()
    {
        logger.LogDebug("RemoveAllAsync");

        await client.DelAsync([.. SettingKeys]);
        //清空缓存key列表
        SettingKeys.Clear();
    }

    public async Task RemoveAsync(string key)
    {
        logger.LogDebug("RemoveAsync: {key}", key);

        await client.DelAsync($"{SettingKeyFormat}{key}");
    }
}
