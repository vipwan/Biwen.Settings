// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:28:46 ISettingStore.cs

using Biwen.Settings.Caching;
using Biwen.Settings.EndpointNotify;

namespace Biwen.Settings.SettingStores;

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

    public async Task SaveAsync<T>(T setting) where T : ISetting, new()
    {
        //Save
        await _settingStore.SaveAsync(setting);
        //Remove Cache
        await _cacheProvider.Value.RemoveAsync(string.Format(Consts.CacheKeyFormat, typeof(T).FullName, _options.Value.ProjectId));
        //Notify
        await _medirator.Value.PublishAsync(setting);

        //todo:如果是分布式环境,需要通知其他节点刷新缓存
        _ = _notifyServices.Value.NotifyConsumerAsync(new NofityDto(typeof(T).FullName!, _options.Value.ProjectId));
    }

    public async Task<T> GetAsync<T>() where T : ISetting, new()
    {
        var retn = await _cacheProvider.Value.GetOrCreateAsync(
            string.Format(Consts.CacheKeyFormat, typeof(T).FullName, _options.Value.ProjectId),
             () => _settingStore.GetAsync<T>().GetAwaiter().GetResult(),
            _options.Value.CacheTime
        );
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
