using Biwen.Settings.Caching;
using Biwen.Settings.EndpointNotify;
using Microsoft.Extensions.Configuration;

namespace Biwen.Settings
{
    /// <summary>
    /// 需要实现自己的SettingManager请继承至BaseSettingManager
    /// </summary>
    public interface ISettingManager
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
    /// SettingManager的装饰器基类
    /// </summary>
    internal sealed class SettingManagerDecorator(
        ISettingManager settingManager,
        IServiceProvider serviceProvider) : ISettingManager
    {
        private readonly ISettingManager _settingManager = settingManager;
        private readonly ICacheProvider _cacheProvider = serviceProvider.GetRequiredService<ICacheProvider>();
        private readonly IMedirator _medirator = serviceProvider.GetRequiredService<IMedirator>();
        private readonly NotifyServices _notifyServices = serviceProvider.GetRequiredService<NotifyServices>();

        private readonly IOptions<SettingOptions> _options = serviceProvider.GetRequiredService<IOptions<SettingOptions>>();

        private static object _locker = new();

        public async void Save<T>(T setting) where T : ISetting, new()
        {
            //Save
            _settingManager.Save(setting);
            //Remove Cache
            await _cacheProvider.RemoveAsync(string.Format(Consts.CacheKeyFormat, typeof(T).FullName, _options.Value.ProjectId));
            //Notify
            await _medirator.PublishAsync(setting);
            //IConfiguration刷新:
            lock (_locker)
            {
                Consts.IsConfigrationChanged = (true, typeof(T).Name);
            }
            //todo:如果是分布式环境,需要通知其他节点刷新缓存
            _ = _notifyServices.NotifyConsumerAsync(new NofityDto(typeof(T).FullName!, _options.Value.ProjectId));
        }

        public T Get<T>() where T : ISetting, new()
        {
            var retn = _cacheProvider.GetOrCreateAsync<T>(
                string.Format(Consts.CacheKeyFormat, typeof(T).FullName, _options.Value.ProjectId),
                 () =>
            {
                return _settingManager.Get<T>();
            }).GetAwaiter().GetResult();
            //if (retn is System.Text.Json.JsonElement)
            //{
            //    var raw = System.Text.Json.JsonSerializer.Serialize(retn);
            //    return System.Text.Json.JsonSerializer.Deserialize<T>(raw)!;
            //}
            return retn!;
        }

        public List<Setting> GetAllSettings()
        {
            return _settingManager.GetAllSettings();
        }

        public Setting? GetSetting(string settingType)
        {
            return _settingManager.GetSetting(settingType);
        }
    }


    /// <summary>
    /// BaseSettingManager
    /// </summary>
    public abstract class BaseSettingManager(ILogger<ISettingManager> logger) : ISettingManager
    {
        protected readonly ILogger<ISettingManager> _logger = logger;

        public abstract void Save<T>(T setting) where T : ISetting, new();

        public abstract T Get<T>() where T : ISetting, new();

        public abstract List<Setting> GetAllSettings();

        public abstract Setting? GetSetting(string settingType);

    }
}