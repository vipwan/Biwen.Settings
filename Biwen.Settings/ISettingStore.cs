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
    void Save<T>(T setting) where T : ISetting, new();
    /// <summary>
    /// 从缓存中获取配置.如果没有则从持久化存储中获取.如果持久层也没有则返回默认值并存入持久层
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Get<T>() where T : ISetting, new();

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
/// SettingStore的装饰器基类
/// </summary>
internal sealed class SettingStoreDecorator(
    ISettingStore settingStore,
    IServiceProvider serviceProvider) : ISettingStore
{
    private readonly ISettingStore _settingStore = settingStore;
    private readonly Lazy<ICacheProvider> _cacheProvider = new(serviceProvider.GetRequiredService<ICacheProvider>());
    private readonly Lazy<IMedirator> _medirator = new(serviceProvider.GetRequiredService<IMedirator>());
    private readonly Lazy<NotifyServices> _notifyServices = new(serviceProvider.GetRequiredService<NotifyServices>());

    private readonly IOptions<SettingOptions> _options = serviceProvider.GetRequiredService<IOptions<SettingOptions>>();

    public async void Save<T>(T setting) where T : ISetting, new()
    {
        //Save
        _settingStore.Save(setting);
        //Remove Cache
        await _cacheProvider.Value.RemoveAsync(string.Format(Consts.CacheKeyFormat, typeof(T).FullName, _options.Value.ProjectId));
        //Notify
        await _medirator.Value.PublishAsync(setting);

        //todo:如果是分布式环境,需要通知其他节点刷新缓存
        _ = _notifyServices.Value.NotifyConsumerAsync(new NofityDto(typeof(T).FullName!, _options.Value.ProjectId));
    }

    public T Get<T>() where T : ISetting, new()
    {
        var retn = _cacheProvider.Value.GetOrCreateAsync<T>(
            string.Format(Consts.CacheKeyFormat, typeof(T).FullName, _options.Value.ProjectId),
            () => _settingStore.Get<T>(),
            _options.Value.CacheTime
        ).GetAwaiter().GetResult();
        return retn!;
    }

    public List<Setting> GetAllSettings()
    {
        return _settingStore.GetAllSettings();
    }

    public Setting? GetSetting(string settingType)
    {
        return _settingStore.GetSetting(settingType);
    }
}


/// <summary>
/// BaseSettingStore
/// </summary>
public abstract class BaseSettingStore(ILogger<ISettingStore> logger) : ISettingStore
{
    protected readonly ILogger<ISettingStore> _logger = logger;

    public abstract void Save<T>(T setting) where T : ISetting, new();

    public abstract T Get<T>() where T : ISetting, new();

    public abstract List<Setting> GetAllSettings();

    public abstract Setting? GetSetting(string settingType);

}