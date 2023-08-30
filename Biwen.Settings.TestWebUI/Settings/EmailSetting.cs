using FluentValidation;

namespace Biwen.Settings.TestWebUI
{

    [Description("邮件配置")]
    public class EmailSetting : SettingBase
    {
        [Description("SMTP服务器")]
        public string Host { get; set; } = "smtp.qq.com";
        public int Port { get; set; } = 465;
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";

        public string From { get; set; } = "";

        public override int Order => 500;


        public class EmailSettingValidtor : AbstractValidator<EmailSetting>
        {
            public EmailSettingValidtor()
            {
                //验证规则
                RuleFor(x => x.Host).NotEmpty().Length(6, 128);
                RuleFor(x => x.Port).NotNull().NotEmpty().GreaterThan(0);
                RuleFor(x => x.UserName).NotNull().NotEmpty().Length(3, 128);
                RuleFor(x => x.Password).NotNull().NotEmpty().Length(3, 128);
                RuleFor(x => x.From).NotNull().NotEmpty().Length(3, 128);
            }
        }

    }


    /// <summary>
    /// 站点基础配置
    /// </summary>
    [Description("站点配置")]
    public class SiteSetting : SettingBase
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

