// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:27:39 ConfigurationMediratorDoneHandler.cs

namespace Biwen.Settings.Extensions.Configuration;

/// <summary>
/// 写入Channel消费
/// </summary>
internal class ConfigurationMediratorDoneHandler(ILogger<ConfigurationMediratorDoneHandler> logger) : IMediratorDoneHandler
{
    public Task OnPublishedAsync<T>(T @event) where T : ISetting, new()
    {
        Events.ConfigrationChangedChannel.Writer.TryWrite((true, typeof(T)));
        logger.LogInformation("Setting Changed: {Name},并通知Configuration刷新!", typeof(T).Name);
        return Task.CompletedTask;
    }
}