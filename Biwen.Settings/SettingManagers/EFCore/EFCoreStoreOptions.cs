using static Biwen.Settings.SettingOptions;

namespace Biwen.Settings.SettingManagers.EFCore
{
    public class EFCoreStoreOptions
    {
        /// <summary>
        /// DbContextType ,it must be inherited from IBiwenSettingsDbContext
        /// </summary>
        public Type DbContextType { get; set; } = null!;

        /// <summary>
        /// 加密的内容配置项
        /// </summary>
        public EncryptionOptions EncryptionOption { get; set; } = new();



    }
}