using Garnet.client;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Biwen.Settings.Caching.Garnet
{
    public class GarnetCacheProvider(IOptions<GarnetClientOptions> optons) : ICacheProvider
    {
        private readonly IOptions<GarnetClientOptions> _options = optons;
        private readonly ConcurrentDictionary<string, object?> Keys = new();

        private const string SettingKeyFormat = "__BiwenSetting__Garnet_{0}";

        public async Task<T?> GetOrCreateAsync<T>(string key, Func<T?> factory, int cacheTime = 86400) where T : ISetting
        {
            Keys.TryAdd(key, null);

            using var db = new GarnetClient(
                address: _options.Value.Host,
                port: _options.Value.Port,
                authUsername: _options.Value.UserName,
                authPassword: _options.Value.Password
                );

            await db.ConnectAsync();

            var value = await db.StringGetAsync(string.Format(SettingKeyFormat, key));

            if (value is null)
            {
                var newVal = factory();
                if (newVal is null)
                {
                    return default;
                }

                await db.StringSetAsync(key: string.Format(SettingKeyFormat, key),
                    value: JsonSerializer.Serialize(newVal));

                return newVal;
            }

            return JsonSerializer.Deserialize<T?>(value);
        }


        public async Task RemoveAsync(string key)
        {
            Keys.TryRemove(key, out _);

            using var db = new GarnetClient(
                            address: _options.Value.Host,
                            port: _options.Value.Port,
                            authUsername: _options.Value.UserName,
                            authPassword: _options.Value.Password
                            );

            await db.ConnectAsync();
            await db.KeyDeleteAsync(string.Format(SettingKeyFormat, key));
        }

        public async Task RemoveAllAsync()
        {
            foreach (var key in Keys.Keys)
            {
                await RemoveAsync(key);
            }
            Keys.Clear();
        }
    }
}