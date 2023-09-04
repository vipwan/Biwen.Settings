
namespace Biwen.Settings.SettingManagers.JsonStore
{
    /// <summary>
    /// JsonStore Options
    /// </summary>
    public class JsonStoreOptions
    {
        /// <summary>
        /// 存储Json文件的路径  默认路径: systemsettings.json
        /// </summary>
        public string JsonPath { get; set; } = "systemsettings.json";

        /// <summary>
        /// 是否格式化Json
        /// </summary>
        public bool FormatJson { get; set; } = true;

    }
}
