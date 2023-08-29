
namespace Biwen.Settings.Domains
{
    public class Setting
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        public string SettingName { get; set; } = null!;
        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; } = 1000;
        /// <summary>
        /// JSON存储
        /// </summary>
        public string? SettingContent { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastModificationTime { get; set; } = DateTime.Now;
    }
}