namespace Biwen.Settings
{
    /// <summary>
    /// 配置接口,使用请继承自:<see cref="SettingBase{T}"/> or <see cref="ValidationSettingBase{T}"/>
    /// </summary>
    public interface ISetting
    {
        string? SettingName { get; }
        int Order { get; }

    }

    internal interface ISettingValidator
    {
        //object RealValidator { get; }

        /// <summary>
        /// 验证当前的Request
        /// </summary>
        /// <returns></returns>
        ValidationResult Validate();

    }
}