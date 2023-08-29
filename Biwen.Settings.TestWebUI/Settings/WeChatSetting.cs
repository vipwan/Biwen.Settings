﻿namespace Biwen.Settings.TestWebUI.Settings
{

    [Description("微信配置")]
    public class WeChatSetting : SettingBase
    {
        [Description("AppId")]
        public string AppId { get; set; } = "wx1234567890";

        [Description("AppSecret")]
        public string AppSecret { get; set; } = "1234567890";

        [Description("Token")]
        public string Token { get; set; } = "1234567890";

        [Description("EncodingAESKey")]
        public string EncodingAESKey { get; set; } = "1234567890";

        public override int Order => 999;

    }
}