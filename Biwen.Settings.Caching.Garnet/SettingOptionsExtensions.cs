namespace Biwen.Settings
{
    using Biwen.Settings.Caching.Garnet;

    public static class SettingOptionsExtensions
    {
        /// <summary>
        /// 使用Garnet缓存
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>

        public static SettingOptions UseCacheOfGarnet(this SettingOptions options)
        {
            options.UseCache<GarnetCacheProvider>();
            return options;
        }
    }
}