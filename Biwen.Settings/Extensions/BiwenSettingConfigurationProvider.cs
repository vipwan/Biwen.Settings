using Biwen.Settings.EndpointNotify;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Threading.Channels;

namespace Biwen.Settings
{
    public static class ConfigurationManagerExtensions
    {
        /// <summary>
        /// 提供对IConfiguration,IOptions的支持
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="autoRefresh"></param>
        /// <returns></returns>
        public static ConfigurationManager AddBiwenSettingConfiguration(
            this ConfigurationManager manager, bool autoRefresh = true)
        {
            if (ServiceRegistration.ServiceProvider is null)
                throw new BiwenException("必须首先注册Biwen.Setting模块,请调用:services.AddBiwenSettings()");

            IConfigurationBuilder configBuilder = manager;
            configBuilder.Add(new BiwenSettingConfigurationSource(autoRefresh));

            var settings = ASS.InAllRequiredAssemblies.ThatInherit(typeof(ISetting)).Where(x => x.IsClass && !x.IsAbstract).ToList();

            //注册ISetting
            settings.ForEach(x =>
            {
                //IOptions DI
                manager?.GetSection(x.Name).Bind(GetSetting(x, ServiceRegistration.ServiceProvider));
            });

            return manager;
        }

        static object GetSetting(Type x, IServiceProvider sp)
        {
            var settingManager = sp.GetRequiredService<ISettingManager>();
            var cache = sp.GetRequiredService<IMemoryCache>();

            //使用缓存避免重复反射
            var md = cache.GetOrCreate($"GenericMethod_{x.FullName}", entry =>
            {
                MethodInfo methodLoad = settingManager.GetType().GetMethod(nameof(settingManager.Get))!;
                MethodInfo generic = methodLoad.MakeGenericMethod(x);
                return generic;
            });
            return md!.Invoke(settingManager, null)!;
        }
    }

    internal sealed class BiwenSettingConfigurationSource(bool autoRefresh = true) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new BiwenSettingConfigurationProvider(autoRefresh);
    }

    internal class BiwenSettingConfigurationProvider : ConfigurationProvider, IDisposable, IAsyncDisposable
    {
        public BiwenSettingConfigurationProvider(bool autoRefresh)
        {
            if (ServiceRegistration.ServiceProvider is null)
            {
                throw new BiwenException("必须首先注册Biwen.Setting模块,请调用:services.AddBiwenSettings()");
            }
            if (autoRefresh)
            {
                StartAlertAsync(cts.Token);
            }
        }

        private CancellationTokenSource cts = new();

        /// <summary>
        /// 使用Channel通知配置变更
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAlertAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _ = await Consts.ConfigrationChangedChannel.Reader.ReadAsync(cancellationToken);
                    Load();
                    //通知配置变更
                    OnReload();
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public override void Load()
        {
            Dictionary<string, string?> dics = [];

            using var scope = ServiceRegistration.ServiceProvider.CreateScope();
            var settingManager = scope.ServiceProvider.GetRequiredService<ISettingManager>();
            var settings = settingManager.GetAllSettings()!;
            foreach (var setting in settings)
            {
                if (setting.SettingContent is null) continue;
                if (JsonNode.Parse(setting.SettingContent) is not JsonObject json) continue;
                foreach (var item in json)
                {
                    //已经存在:
                    dics.TryAdd($"{setting.SettingName}:{item.Key}", item.Value?.ToString());
                }
            }

            Data = dics;
        }

        public void Dispose()
        {
            cts.Cancel();
            Consts.ConfigrationChangedChannel.Writer.Complete();
        }

        public ValueTask DisposeAsync()
        {
            cts.Cancel();
            Consts.ConfigrationChangedChannel.Writer.Complete();
            return ValueTask.CompletedTask;
        }
    }
}