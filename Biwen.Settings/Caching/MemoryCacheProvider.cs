using Microsoft.Extensions.Caching.Memory;
namespace Biwen.Settings.Caching
{
    /// <summary>
    /// 默认的缓存提供者,使用内存缓存
    /// </summary>
    public sealed class MemoryCacheProvider(IMemoryCache cache) : ICacheProvider
    {
        private readonly IMemoryCache _cache = cache;

        public async Task<object?> GetOrCreateAsync(string key, Func<Task<object?>> factory, int cacheTime = 86400)
        {
            return await _cache.GetOrCreateAsync(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheTime);
                return factory();
            });
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}