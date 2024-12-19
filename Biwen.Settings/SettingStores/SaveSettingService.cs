// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:30:37 SaveSettingService.cs

namespace Biwen.Settings.SettingStores;

internal class SaveSettingService(ILogger<SaveSettingService> logger, ISettingStore settingStore, IAsyncContext<SettingRecord> asyncContext)
{
    public async Task SaveSettingAsync()
    {
        if (!asyncContext.TryGet(out var record))
        {
            logger.LogWarning("No SettingRecord in AsyncContext");
            return;
        }
        //Save
        var mdSave = settingStore.GetType().GetMethod(nameof(ISettingStore.Save))!.MakeGenericMethod(record?.SettingType!);
        mdSave.Invoke(settingStore, [record?.Setting!]);
        await Task.CompletedTask;
    }

}