
using FluentValidation;

namespace Biwen.Settings.TestWebUI
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

        public class WeChatSettingValidtor : AbstractValidator<WeChatSetting>
        {
            public WeChatSettingValidtor()
            {
                //验证规则
                RuleFor(x => x.AppId).NotEmpty().Length(12, 32);
                RuleFor(x => x.AppSecret).NotNull().NotEmpty().Length(12, 128);
            }
        }

    }

    public class WeChatSetting2 : WeChatSetting
    {

    }
    public class WeChatSetting3 : WeChatSetting
    {

    }
    public class WeChatSetting4 : WeChatSetting
    {

    }
    public class WeChatSetting5 : WeChatSetting
    {

    }
    public class WeChatSetting6 : WeChatSetting
    {

    }
    public class WeChatSetting7 : WeChatSetting
    {

    }
    public class WeChatSetting8 : WeChatSetting
    {

    }
    public class WeChatSetting9 : WeChatSetting
    {

    }
}