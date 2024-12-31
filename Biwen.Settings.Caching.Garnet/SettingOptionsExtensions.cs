// Licensed to the Biwen.Settings.Caching.Garnet under one or more agreements.
// The Biwen.Settings.Caching.Garnet licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Biwen.Settings;
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配

using Biwen.Settings.Caching.Garnet;

public static class SettingOptionsExtensions
{
    /// <summary>
    /// 使用Garnet缓存
    /// </summary>
    /// <param name="options"></param>
    /// <param name="cacheTime">缓存时间,默认86400秒</param>
    /// <returns></returns>

    public static SettingOptions UseCacheOfGarnet(this SettingOptions options, int cacheTime = 86400)
    {
        options.UseCache<GarnetCacheProvider>(cacheTime);
        return options;
    }
}