// Licensed to the Biwen.Settings.Caching.Garnet under one or more agreements.
// The Biwen.Settings.Caching.Garnet licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Biwen.Settings;

using Biwen.Settings.Redis.Caching;

public static class SettingOptionsExtensions
{
    /// <summary>
    /// 使用CsRedis缓存
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>

    public static SettingOptions UseCacheOfRedis(this SettingOptions options)
    {
        options.UseCache<RedisCacheProvider>();
        return options;
    }
}