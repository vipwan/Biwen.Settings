using FluentValidation;

namespace Biwen.Settings.OCTestWebUI.Settings
{
    [Description("Hello配置")]
    public class HelloSetting : ValidationSettingBase<HelloSetting>
    {
        [Description("名称")]
        public string? Name { get; set; } = "Hello";

        public HelloSetting()
        {
            RuleFor(x => x.Name).NotEmpty().Length(8, 32);
        }

        /// <summary>
        /// Notify
        /// </summary>
        public class HelloSettingNotify : BaseNotify<HelloSetting>
        {
            private readonly ILogger<HelloSettingNotify> _logger;
            public HelloSettingNotify(ILogger<HelloSettingNotify> logger)
            {
                _logger = logger;
            }
            public override async Task NotifyAsync(HelloSetting setting)
            {
                _logger.LogInformation("Hello配置发生变更!");
                await Task.CompletedTask;
            }
        }
    }
    public class TestService
    {
        private readonly HelloSetting _helloSetting;
        public TestService(HelloSetting helloSetting)
        {
          _helloSetting = helloSetting;
        }

        public string GetHelloName()
        {
            return _helloSetting.Name!;
        }

    }

}