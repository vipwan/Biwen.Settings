namespace Biwen.Settings.TestWebUI.Settings
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

    }
}
