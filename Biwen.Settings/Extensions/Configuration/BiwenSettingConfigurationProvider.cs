// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:27:32 BiwenSettingConfigurationProvider.cs

using Microsoft.Extensions.Configuration;
using System.Threading.Channels;

namespace Biwen.Settings.Extensions.Configuration;

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
        var settingStore = scope.ServiceProvider.GetRequiredService<ISettingStore>();
        var settings = settingStore.GetAllSettings()!;
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