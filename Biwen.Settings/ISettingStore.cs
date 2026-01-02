// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:28:46 ISettingStore.cs

using Biwen.Settings.Caching;
using Biwen.Settings.EndpointNotify;

namespace Biwen.Settings;

/// <summary>
/// 需要实现自己的SettingStore请继承至BaseSettingStore
/// </summary>
public interface ISettingStore
{
    /// <summary>
    /// 持久化存储.并刷新缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="setting"></param>
    Task SaveAsync<T>(T setting) where T : ISetting, new();
    /// <summary>
    /// 从缓存中获取配置.如果没有则从持久化存储中获取.如果持久层也没有则返回默认值并存入持久层
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> GetAsync<T>() where T : ISetting, new();

    /// <summary>
    /// 从持久化存储中获取配置
    /// </summary>
    /// <returns></returns>
    List<Setting> GetAllSettings();

    /// <summary>
    /// 从持久层获取指定类型的配置
    /// </summary>
    /// <param name="settingType"></param>
    /// <returns></returns>
    Setting? GetSetting(string settingType);

}

/// <summary>
/// BaseSettingStore
/// </summary>
public abstract class BaseSettingStore(ILogger<ISettingStore> logger) : ISettingStore
{
    protected readonly ILogger<ISettingStore> _logger = logger;

    public abstract Task SaveAsync<T>(T setting) where T : ISetting, new();

    public abstract Task<T> GetAsync<T>() where T : ISetting, new();

    public abstract List<Setting> GetAllSettings();

    public abstract Setting? GetSetting(string settingType);

}