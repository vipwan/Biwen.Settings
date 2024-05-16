using Microsoft.Extensions.Configuration;
namespace Biwen.Settings.Extensions
{
    public static class ConfigurationManagerExtensions
    {
        /// <summary>
        /// 提供对IConfiguration的支持
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="autoRefresh"></param>
        /// <returns></returns>
        internal static ConfigurationManager AddBiwenSettingConfiguration(
            this ConfigurationManager manager, bool autoRefresh = true)
        {
            IConfigurationBuilder configBuilder = manager;
            configBuilder.Add(new BiwenSettingConfigurationSource(autoRefresh));
            return manager;
        }
    }

    internal sealed class BiwenSettingConfigurationSource(bool autoRefresh = true) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new BiwenSettingConfigurationProvider(autoRefresh);
    }

    internal class BiwenSettingConfigurationProvider : ConfigurationProvider, IDisposable
    {
        public BiwenSettingConfigurationProvider(bool autoRefresh)
        {
            if (ServiceRegistration.ServiceProvider is null)
            {
                throw new BiwenException("必须首先注册Biwen.Setting模块,请调用:services.AddBiwenSettings()");
            }
            if (autoRefresh)
            {
                t = new System.Timers.Timer(TimeSpan.FromMilliseconds(100)) { Enabled = true };
                t.Elapsed += T_Elapsed;
                t.Start();
            }
        }

        private void T_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_lock)
                if (EndpointNotify.Consts.IsConfigrationChanged.IsChanged)
                {
                    Load();
                    EndpointNotify.Consts.IsConfigrationChanged = (false, null);
                    //通知配置变更
                    OnReload();
                }
        }

        private static object _lock = new();
        System.Timers.Timer t = null!;

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
            t?.Dispose();
        }

    }
}