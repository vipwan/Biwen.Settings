
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Biwen.Settings.TestWebUI
{
    [Description("Github配置")]
    public class GithubSetting : SettingBase
    {

        [Description("Github用户名")]
        public string? UserName { get; set; } = "vipwan";

        [Description("Github仓库")]
        public string? Repository { get; set; } = "Biwen.Settings";

        [Description("Github Token")]
        public string? Token { get; set; } = "";

        /// <summary>
        /// 短名称
        /// </summary>
        public override string? SettingName => this.GetType().Name;

        public override int Order => 500;

        public class GithubSettingValidtor : AbstractValidator<GithubSetting>
        {
            public GithubSettingValidtor()
            {
                //验证规则
                RuleFor(x => x.UserName).NotEmpty().Length(3, 128);
                RuleFor(x => x.Repository).NotNull().NotEmpty().Length(3, 128);
                RuleFor(x => x.Token).NotNull().NotEmpty().Length(3, 128);
            }
        }
    }



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

    /// <summary>
    /// 内置验证器的配置项,推荐使用这种方式
    /// </summary>
    [Description("内置验证器的配置项测试")]
    public class TestAutoValidSetting : ValidationSettingBase<TestAutoValidSetting>
    {

        public string Name { get; set; } = "Hello"!;

        public TestAutoValidSetting()
        {
            //构造函数中添加验证规则
            RuleFor(x => x.Name).NotEmpty().Length(8, 32);

        }

        override public int Order => 600;
    }


    public class WeChatSetting2 : WeChatSetting
    {
        override public int Order => 1000;
    }
    public class WeChatSetting3 : WeChatSetting
    {
        override public int Order => 1000;
    }
    public class WeChatSetting4 : WeChatSetting
    {
        override public int Order => 1000;
    }
    public class WeChatSetting5 : WeChatSetting
    {
        override public int Order => 1000;
    }
    public class WeChatSetting6 : WeChatSetting
    {
        override public int Order => 1000;
    }
    public class WeChatSetting7 : WeChatSetting
    {
        override public int Order => 1000;
    }
    public class WeChatSetting8 : WeChatSetting
    {
        override public int Order => 1000;
    }
    public class WeChatSetting9 : WeChatSetting
    {
        override public int Order => 1000;
    }
}