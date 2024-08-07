using Microsoft.Extensions.Caching.Memory;

namespace Biwen.Settings.Caching
{
    /// <summary>
    /// 默认的缓存提供者,使用内存缓存
    /// </summary>
    public sealed class MemoryCacheProvider(IMemoryCache cache) : ICacheProvider
    {
        private readonly IMemoryCache _cache = cache;

        private static readonly ConcurrentDictionary<string, object?> Keys = new();

        public async Task<T?> GetOrCreateAsync<T>(string key, Func<T?> factory, int cacheTime = 86400) where T : ISetting
        {
            Keys.TryAdd(key, null);

            return await _cache.GetOrCreateAsync<T>(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheTime);
                var @default = factory();
                return await Task.FromResult(@default!);
            });
        }

        public Task RemoveAllAsync()
        {
            foreach (var key in Keys.Keys)
            {
                _cache.Remove(key);
            }
            Keys.Clear();
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            //删除Key
            Keys.TryRemove(key, out _);

            _cache.Remove(key);
            return Task.CompletedTask;
        }

    }
}