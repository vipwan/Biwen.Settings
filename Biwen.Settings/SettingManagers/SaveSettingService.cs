namespace Biwen.Settings.SettingManagers
{
    internal class SaveSettingService(ILogger<SaveSettingService> logger, ISettingManager settingManager, IAsyncContext<SettingRecord> asyncContext)
    {
        public async Task SaveSettingAsync()
        {
            var flag = asyncContext.TryGet(out var record);
            if (!flag)
            {
                logger.LogWarning("No SettingRecord in AsyncContext");
                return;
            }
            //Save
            var mdSave = settingManager.GetType().GetMethod(nameof(ISettingManager.Save))!.MakeGenericMethod(record?.SettingType!);
            mdSave.Invoke(settingManager, [record?.Setting!]);
            await Task.CompletedTask;
        }

    }
}