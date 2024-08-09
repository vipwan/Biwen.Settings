using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace Biwen.Settings
{
    /// <summary>
    /// 继承此类的配置项，将会被自动注册到配置中心,如果需要验证,请继承自ValidationSettingBase<T>
    /// </summary>
    public abstract class SettingBase<T> : ISetting,
    #region fit for IOptions
        IValidateOptions<T>, IConfigureOptions<T>, IPostConfigureOptions<T> where T : class, ISetting, new()
        #endregion
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        [JsonIgnore]
        public virtual string? SettingName => GetType().Name;
        /// <summary>
        /// 默认的排序
        /// </summary>
        [JsonIgnore]
        public virtual int Order => 1000;

        #region IOptions 兼容

        public void Configure(T options)
        {
            using var scope = ServiceRegistration.ServiceProvider.CreateScope();
            var settingManager = scope.ServiceProvider.GetRequiredService<ISettingManager>();
            var setting = settingManager.Get<T>();
            //将配置项的值赋值给options
            //options = setting;
            scope.ServiceProvider.GetService<IConfiguration>()?.Bind(typeof(T).Name, options);
        }

        public virtual ValidateOptionsResult Validate(string? name, T options)
        {
            return ValidateOptionsResult.Success;
        }

        public void PostConfigure(string? name, T options)
        {
            using var scope = ServiceRegistration.ServiceProvider.CreateScope();
            var settingManager = scope.ServiceProvider.GetRequiredService<ISettingManager>();
            var setting = settingManager.Get<T>();
            //将配置项的值赋值给options
            //options = setting;
            scope.ServiceProvider.GetService<IConfiguration>()?.Bind(typeof(T).Name, options);
        }

        #endregion

    }
}