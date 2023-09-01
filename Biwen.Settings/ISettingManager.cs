using Biwen.Settings.Caching;

namespace Biwen.Settings
{
    /// <summary>
    /// 需要实现自己的SettingManager请继承至BaseSettingManager
    /// 不要直接使用ISettingManager,否则ICacheProvider需要自行管理
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

    }


    /// <summary>
    /// BaseSettingManager
    /// </summary>
    public abstract class BaseSettingManager : ISettingManager
    {

        protected readonly ICacheProvider _cacheProvider;
        protected readonly ILogger<ISettingManager> _logger;

        public BaseSettingManager(ICacheProvider cacheProvider, ILogger<ISettingManager> logger)
        {
            _cacheProvider = cacheProvider;
            _logger = logger;
        }


        public virtual void Save<T>(T setting) where T : ISetting, new()
        {
            throw new NotImplementedException();
        }

        public virtual T Get<T>() where T : ISetting, new()
        {
            throw new NotImplementedException();
        }

        public virtual List<Setting> GetAllSettings()
        {
            throw new NotImplementedException();
        }
    }
}