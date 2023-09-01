namespace Biwen.Settings.Caching
{

    /// <summary>
    /// 缓存提供者,应用可支持多种缓存,如内存缓存,Redis缓存等 实现分布式缓存
    /// </summary>
    public interface ICacheProvider
    {

        /// <summary>
        /// 创建或获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <param name="cacheTime">缓存周期秒:seconds </param>
        /// <returns></returns>
        object GetOrCreate(string key, Func<object> factory, int cacheTime = 24 * 60 * 60);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);

    }
}