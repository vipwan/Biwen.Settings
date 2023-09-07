using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Biwen.Settings.OC.Settings
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

        public override int Order => 500;

        /// <summary>
        /// 常规的验证器,不推荐使用这种方式,会全局注入IValidator<T>
        /// </summary>
        public class WeChatSettingValidtor : AbstractValidator<WeChatSetting>
        {
            public WeChatSettingValidtor()
            {
                //验证规则
                RuleFor(x => x.AppId).NotEmpty().Length(12, 32);
                RuleFor(x => x.AppSecret).NotNull().NotEmpty().Length(12, 128);
            }
        }


        public class WeChatSettingNotify : BaseNotify<WeChatSetting>
        {
            private readonly ILogger<WeChatSettingNotify> _logger;

            public WeChatSettingNotify(ILogger<WeChatSettingNotify> logger)
            {
                _logger = logger;
            }

            public override async Task NotifyAsync(WeChatSetting setting)
            {
                _logger.LogInformation("微信配置发生变更!");
                await Task.CompletedTask;
            }
        }

    }

}
