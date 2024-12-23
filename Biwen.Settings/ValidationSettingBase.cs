﻿// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:31:05 ValidationSettingBase.cs

using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Biwen.Settings;

/// <summary>
/// 自带验证器的配置项
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ValidationSettingBase<T> : SettingBase<T>, ISettingValidator where T : class, ISetting, new()
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

    public override ValidateOptionsResult Validate(string? name, T options)
    {
        var result = Validate();
        return result switch
        {
            { IsValid: true } => ValidateOptionsResult.Success,
            { IsValid: false } => ValidateOptionsResult.Fail(result.Errors.Select(x => x.ErrorMessage)),
            _ => ValidateOptionsResult.Success
        };
    }

    #region 内部属性
    /// <summary>
    /// 全局仅有一个T的内部验证器
    /// </summary>
    [JsonIgnore]
    private readonly InnerValidator Validator = new();

    //[JsonIgnore]
    //[Obsolete("请使用Validate(),以同时兼容DataAnnotation和FluentValidation,请注意该属性未来会被移除!", false)]
    //public object RealValidator => Validator;

    public ValidationResult Validate()
    {
        var req = (T)MemberwiseClone();
        return Validator.Validate(req);

        #region 重写PreValidate实现

        ////ms内建的DataAnnotations验证器
        //var context = new MSDA.ValidationContext(req);
        //var validationResults = new List<MSDA.ValidationResult>();
        //var defaultFlag = MSDA.Validator.TryValidateObject(req, context, validationResults, true);

        ////FluentValidation验证器
        //var fluentValidationResult = Validator.Validate(req);

        //if (!defaultFlag)
        //{
        //    fluentValidationResult.Errors.AddRange(validationResults.Select(x => new ValidationFailure(x.MemberNames.FirstOrDefault(), x.ErrorMessage)));
        //}
        ////var method = typeof(InnerValidator).GetMethods().First(x => x.Name == nameof(IValidator.Validate));
        ////return (method!.Invoke(Validator, new object[] { this }) as ValidationResult)!;
        //return fluentValidationResult;

        #endregion
    }

    #endregion

    private class InnerValidator : AbstractValidator<T>
    {
        /// <summary>
        /// 类型是否标注验证特性
        /// </summary>
        static readonly ConcurrentDictionary<string, bool> CachedAnnotationTypes = new();

        private static bool HasAnnotationAttr
        {
            get
            {
                var typeName = typeof(T).FullName!;
                return CachedAnnotationTypes.GetOrAdd(typeName, _ =>
                 {
                     var has = typeof(T).GetProperties().Any(
                         prop =>
                         prop.GetCustomAttributes(true).Any(x => x is MSDA.ValidationAttribute));
                     return has;
                 });
            }
        }

        protected override bool PreValidate(ValidationContext<T> context, ValidationResult result)
        {
            //用于提升性能,如果没有DataAnnotation,则不再执行DataAnnotation的验证
            if (!HasAnnotationAttr)
            {
                return base.PreValidate(context, result);
            }

            var req = context.InstanceToValidate!;
            //ms内建的DataAnnotations验证器
            var mc = new MSDA.ValidationContext(req);
            var validationResults = new List<MSDA.ValidationResult>();
            var flag = MSDA.Validator.TryValidateObject(req, mc, validationResults, true);
            if (!flag)
            {
                result.Errors.AddRange(validationResults.Select(x => new ValidationFailure(x.MemberNames.FirstOrDefault(), x.ErrorMessage)));
            }
            return base.PreValidate(context, result);
        }


    }
}