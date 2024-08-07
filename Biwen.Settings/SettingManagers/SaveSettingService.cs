namespace Biwen.Settings.SettingManagers
{
    internal class SaveSettingService(ILogger<SaveSettingService> logger, ISettingManager settingManager, IAsyncContext<SettingRecord> asyncContext)
    {
        public async Task SaveSettingAsync()
        {
            if (!asyncContext.TryGet(out var record))
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