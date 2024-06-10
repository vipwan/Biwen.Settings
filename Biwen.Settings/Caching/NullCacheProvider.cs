namespace Biwen.Settings.Caching
{
    /// <summary>
    /// 默认的缓存提供者,不做任何缓存
    /// </summary>
    internal sealed class NullCacheProvider : ICacheProvider
    {
        public Task<T?> GetOrCreateAsync<T>(string key, Func<T?> factory, int cacheTime = 86400) where T : ISetting
        {
            return Task.FromResult(factory());
        }

        public Task RemoveAllAsync()
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            return Task.CompletedTask;
        }
    }
}