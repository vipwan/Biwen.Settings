using Microsoft.Extensions.Caching.Memory;
namespace Biwen.Settings.Caching
{
    /// <summary>
    /// 默认的缓存提供者,使用内存缓存
    /// </summary>
    public sealed class MemoryCacheProvider(IMemoryCache cache) : ICacheProvider
    {
        private readonly IMemoryCache _cache = cache;

        public object GetOrCreate(string key, Func<object> factory, int cacheTime = 86400)
        {
            return _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheTime);
                return factory();
            })!;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}