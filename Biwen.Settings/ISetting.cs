using FluentValidation;
using FluentValidation.Internal;
using System.Linq.Expressions;
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

    interface ISettingValidator
    {
        object RealValidator { get; }
    }

    /// <summary>
    /// 自带验证器的配置项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValidationSettingBase<T> : SettingBase, ISettingValidator
    {
        /// <summary>
        /// 添加验证规则
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IRuleBuilderInitial<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return Validator.RuleFor(expression);
        }

        #region 内部属性
        /// <summary>
        /// 全局仅有一个T的内部验证器
        /// </summary>
        [JsonIgnore]
        private readonly InnerValidator Validator = new();

        [JsonIgnore]
        public object RealValidator => Validator;

        #endregion

        private class InnerValidator : AbstractValidator<T>
        {
        }
    }
}