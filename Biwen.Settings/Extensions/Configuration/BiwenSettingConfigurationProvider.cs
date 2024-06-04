﻿using Microsoft.Extensions.Configuration;
using System.Threading.Channels;

namespace Biwen.Settings.Extensions.Configuration
{
    internal class Events
    {
        /// <summary>
        /// Channel队列
        /// </summary>
        public static readonly Channel<(bool IsChanged, Type SettingType)> ConfigrationChangedChannel = Channel.CreateUnbounded<(bool IsChanged, Type SettingType)>();
    }

    internal sealed class BiwenSettingConfigurationSource(bool autoRefresh = true) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new BiwenSettingConfigurationProvider(autoRefresh);
    }

    internal class BiwenSettingConfigurationProvider : ConfigurationProvider, IDisposable, IAsyncDisposable
    {
        public BiwenSettingConfigurationProvider(bool autoRefresh)
        {
            if (Settings.ServiceRegistration.ServiceProvider is null)
            {
                throw new BiwenException("必须首先注册Biwen.Setting模块,请调用:services.AddBiwenSettings()");
            }
            if (autoRefresh)
            {
                StartAlertAsync(cts.Token);
            }
        }

        private readonly CancellationTokenSource cts = new();

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
                    var (_, SettingType) = await Events.ConfigrationChangedChannel.Reader.ReadAsync(cancellationToken);
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

            using var scope = Settings.ServiceRegistration.ServiceProvider.CreateScope();
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
            Events.ConfigrationChangedChannel.Writer.Complete();
        }

        public ValueTask DisposeAsync()
        {
            cts.Cancel();
            Events.ConfigrationChangedChannel.Writer.Complete();
            return ValueTask.CompletedTask;
        }
    }
}