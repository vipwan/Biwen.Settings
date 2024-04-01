using Biwen.Settings.Caching;
using Biwen.Settings.SettingManagers.EFCore;
using Biwen.Settings.SettingManagers.JsonStore;

namespace Biwen.Settings
{
    public static class SettingOptionsExtensions
    {

        /// <summary>
        /// 使用内存缓存
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>

        public static SettingOptions UseCacheOfMemory(this SettingOptions options)
        {
            options.UseCache<MemoryCacheProvider>();
            return options;
        }

        /// <summary>
        /// Default不使用缓存
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static SettingOptions UseCacheOfNull(this SettingOptions options)
        {
            options.UseCache<NullCacheProvider>();
            return options;
        }

        /// <summary>
        /// 使用EntityFmeworkCore持久化配置项
        /// </summary>
        /// <param name="options"></param>
        /// <param name="storePptions"></param>
        /// <returns></returns>
        public static SettingOptions UseStoreOfEFCore(this SettingOptions options, Action<EFCoreStoreOptions>? storeOptions = null)
        {
            options.UseSettingManager<EntityFrameworkCoreSettingManager, Action<EFCoreStoreOptions>?>(storeOptions);
            return options;
        }

        public static SettingOptions UserStoreOfJsonFile(this SettingOptions options, Action<JsonStoreOptions>? storeOptions = null)
        {
            options.UseSettingManager<JsonStoreSettingManager, Action<JsonStoreOptions>?>(storeOptions);
            return options;
        }
    }

}