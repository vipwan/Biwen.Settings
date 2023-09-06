namespace Biwen.Settings.SettingManagers.EFCore
{
    public class EFCoreStoreOptions
    {
        /// <summary>
        /// DbContextType ,it must be inherited from IBiwenSettingsDbContext
        /// </summary>
        public Type DbContextType { get; set; } = null!;

    }
}