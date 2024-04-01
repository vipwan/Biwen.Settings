namespace Biwen.Settings.Caching
{
    /// <summary>
    /// 默认的缓存提供者,不做任何缓存
    /// </summary>
    internal sealed class NullCacheProvider : ICacheProvider
    {
        public async Task<object?> GetOrCreateAsync(string key, Func<Task<object?>> factory, int cacheTime = 86400)
        {
            return await factory();
        }

        public async Task RemoveAsync(string key)
        {
            await Task.CompletedTask;
        }
    }
}