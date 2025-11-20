using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Biwen.Settings.Server
{

    [Description("邮件配置")]
    public class EmailSetting : ValidationSettingBase<EmailSetting>
    {
        [Description("SMTP服务器")]
        [Required, StringLength(128, MinimumLength = 6)]
        public string Host { get; set; } = "smtp.qq.com";

        /// <summary>
        /// DataAnnotations测试,300-699
        /// </summary>
        [Range(300, 699), Required]
        public int Port { get; set; } = 465;

        [StringLength(128, MinimumLength = 3), Required]
        public string UserName { get; set; } = "";

        [StringLength(128, MinimumLength = 3), Required]
        public string Password { get; set; } = "";

        [StringLength(128, MinimumLength = 3), Required]
        public string From { get; set; } = "";

        public override int Order => 500;

    }

    /// <summary>
    /// 站点基础配置
    /// </summary>
    [Description("站点配置")]
    public class SiteSetting : ValidationSettingBase<SiteSetting>
    {
        [Description("站点作者")]
        public string? Author { get; set; } = "万雅虎";

        [Description("站点名称")]
        public string? SiteName { get; set; } = "Biwen.Setting";

        [Description("站点地址")]
        public string? SiteUrl { get; set; } = "https://github.com/vipwan/Biwen.Settings";
        [Description("站点Logo")]
        public string? SiteLogo { get; set; } = "https://avatars.githubusercontent.com/u/1026229?s=200&v=4";
        [Description("站点描述")]
        public string? SiteDescription { get; set; } = "Biwen.Settings 组件配置模块";
        [Description("站点关键字")]
        public string? SiteKeywords { get; set; } = "Biwen.Settings";
    }

}

