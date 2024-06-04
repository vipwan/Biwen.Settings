using Microsoft.AspNetCore.Http;
using System.Dynamic;

namespace Biwen.Settings.Mvc
{
    /// <summary>
    /// 验证DTO的Filter
    /// </summary>
    internal class ValidDtoFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var id = context.Arguments[3]!.ToString(); //context.HttpContext.GetRouteValue("id") as string;
            if (string.IsNullOrEmpty(id)) return Results.NotFound();
            var type = ASS.InAllRequiredAssemblies.FirstOrDefault(x => x.FullName == id);
            if (type == null) return Results.NotFound();
            //json object ->mapper
            //EnableBuffering()允许多次调用Stream,并且Position重置为0.
            context.HttpContext.Request.EnableBuffering();
            //json ->dto
            if ((await context.HttpContext!.Request.ReadFromJsonAsync<ExpandoObject>()) is not IDictionary<string, object> dto)
            {
                return Results.BadRequest();
            }
            context.HttpContext.Request.Body.Position = 0;//Reset Position= 0. 

            //提供Patch部分更新支持:
            var setting = context!.HttpContext!.RequestServices.GetService(type!);
            if (setting == null) return Results.NotFound();

            foreach (PropertyInfo prop in type!.GetProperties())
            {
                //SetMethod 判断
                if (prop?.SetMethod == null)
                    continue;
                if (!dto!.ContainsKey(prop.Name))
                    continue;
                //当前类型必须能转换String
                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;
                //当前类型必须能转换传递的参数值
                var strValue = dto[prop.Name];
                if (strValue == null)
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).IsValid(strValue.ToString()!))
                    continue;
                //转换
                var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(strValue.ToString()!);
                //赋值
                prop.SetValue(setting, value);
            }
            var option = (context.Arguments[1] as IOptions<SettingOptions>)!.Value;
            if (option.AutoFluentValidationOption.Enable)
            {
                //继承至ValidationSettingBase<T>的情况
                if (type.BaseType!.IsConstructedGenericType && type.BaseType!.GenericTypeArguments.Any(x => x == type))
                {
                    var x = setting as ISettingValidator ?? throw new BiwenException($"ISettingValidator is Null!");
                    var vResult = x.Validate();
                    if (!vResult.IsValid)
                    {
                        return Results.ValidationProblem(vResult.ToDictionary());
                    }
                }

                //验证DTO
                (bool Succesed, IDictionary<string, string[]>? Errors) Valid(object validator)
                {
                    var md = validator.GetType().GetMethods().First(x => !x.IsGenericMethod && x.Name == nameof(IValidator.Validate));

                    //验证不通过的情况
                    if (md!.Invoke(validator, [setting]) is ValidationResult result && !result!.IsValid)
                    {
                        return (false, result.ToDictionary());
                    }
                    return (true, null);
                }

                //存在验证器的情况
                var validator = context.HttpContext!.RequestServices.GetService(typeof(IValidator<>).MakeGenericType(type));
                if (validator != null)
                {
                    var (Succesed, Errors) = Valid(validator);
                    if (!Succesed)
                    {
                        return Results.ValidationProblem(Errors!);
                    }
                }
            }
            return await next(context);
        }
    }
}
