using FluentValidation.Internal;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Biwen.Settings
{
    /// <summary>
    /// 配置接口,使用请继承自SettingBase
    /// </summary>
    public interface ISetting
    {
        string? SettingName { get; }
        int Order { get; }

    }
    /// <summary>
    /// 继承此类的配置项，将会被自动注册到配置中心
    /// </summary>
    public abstract class SettingBase : ISetting
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
    }
}