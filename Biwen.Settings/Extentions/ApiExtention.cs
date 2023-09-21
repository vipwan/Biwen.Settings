using Biwen.Settings.Encryption;
using Biwen.Settings.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Dynamic;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApiExtention
    {
        /// <summary>
        /// 注册BiwenSetting Api
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="routePrefix"></param>
        /// <returns></returns>
        public static RouteGroupBuilder MapBiwenSettingApi(this IEndpointRouteBuilder endpoint, string routePrefix = "biwensetting/api")
        {
            //group
            var group = endpoint.MapGroup(routePrefix);
            //auth
            group.AddEndpointFilter<MinimalAuthFilter>();
            //all
            group.MapGet("all", (ISettingManager settingManager, IEncryptionProvider encryptionProvider)
                =>
            {
                var all = settingManager.GetAllSettings();
                return Results.Json(all.Select(x => x.MapperToDto(encryptionProvider)));
            });
            //get
            group.MapGet("get/{id}", (ISettingManager settingManager, string id, IEncryptionProvider encryptionProvider)
                =>
            {
                if (string.IsNullOrEmpty(id)) return Results.NotFound();
                var setting = settingManager.GetSetting(id);
                return setting == null ? Results.NotFound() : Results.Json(setting.MapperToDto(encryptionProvider));
            });
            //set/{id}
            group.MapPost("set/{id}", async (ISettingManager settingManager, IOptions<SettingOptions> options, IHttpContextAccessor ctx, string id)
                =>
            {
                //ValidDtoFilter Before

                var type = ASS.InAllRequiredAssemblies.First(x => x.FullName == id);
                //json ->dto
                if ((await ctx.HttpContext!.Request.ReadFromJsonAsync<ExpandoObject>()) is not IDictionary<string, object> dto)
                {
                    return Results.BadRequest();
                }
                //提供Patch部分更新支持:
                var setting = ctx!.HttpContext!.RequestServices.GetService(type!)!;
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
                    //Convert
                    var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(strValue.ToString()!);
                    //Set
                    prop.SetValue(setting, value);
                }
                //Save
                var mdSave = settingManager.GetType().GetMethod(nameof(ISettingManager.Save))!.MakeGenericMethod(type!);
                mdSave.Invoke(settingManager, new[] { setting! });
                return Results.Ok(setting);
            }).AddEndpointFilter<ValidDtoFilter>();

            return group;
        }

        /// <summary>
        /// Dto
        /// </summary>
        /// <param name="SettingType"></param>
        /// <param name="SettingName"></param>
        /// <param name="Description"></param>
        /// <param name="SettingContent"></param>
        /// <param name="LastModificationTime"></param>
        record SettingDto(string SettingType, string SettingName, string? Description, string? SettingContent, DateTime LastModificationTime);

        #region helper

        /// <summary>
        /// Mapper
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        private static SettingDto MapperToDto(this Setting setting, IEncryptionProvider encryptionProvider)
        {
            return new SettingDto(
                               setting.SettingType,
                               setting.SettingName,
                               setting.Description,
                               setting.SettingContent,
                               setting.LastModificationTime);
        }

        /// <summary>
        /// (,)转换为JSON匿名类型
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static dynamic ToExpandoObject(this List<(string, string)> values)
        {
            dynamic obj = new ExpandoObject();
            foreach (var item in values)
            {
                //如果重复,则忽略
                ((IDictionary<string, object>)obj).TryAdd(item.Item1, item.Item2);
            }
            return obj;
        }

        #endregion

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
                    //验证DTO
                    (bool, IDictionary<string, string[]>?) Valid(MethodInfo? md, object validator)
                    {
                        //验证不通过的情况
                        if (md!.Invoke(validator, new[] { setting }) is ValidationResult result && !result!.IsValid)
                        {
                            return (false, result.ToDictionary());
                        }
                        return (true, null);
                    }

                    //存在验证器的情况
                    var validator = context.HttpContext!.RequestServices.GetService(
                        serviceType: typeof(IValidator<>).MakeGenericType(type));
                    if (validator != null)
                    {
                        var md = validator.GetType().GetMethods().First(
                            x => !x.IsGenericMethod && x.Name == nameof(IValidator.Validate));
                        var vResult = Valid(md, validator);
                        if (!vResult.Item1)
                        {
                            return Results.ValidationProblem(vResult.Item2!);
                        }
                    }

                    //继承至ValidationSettingBase<T>的情况
                    if (type.BaseType!.IsConstructedGenericType && type.BaseType!.GenericTypeArguments.Any(x => x == type))
                    {
                        var x = dto as ISettingValidator ?? throw new BiwenException($"ISettingValidator is Null!");
                        var md = x.RealValidator.GetType().GetMethods().First(
                            x => !x.IsGenericMethod && x.Name == nameof(IValidator.Validate));
                        //验证不通过的情况
                        var vResult = Valid(md, x.RealValidator);
                        if (!vResult.Item1)
                        {
                            return Results.ValidationProblem(vResult.Item2!);
                        }
                    }
                }
                return await next(context);
            }
        }
    }
}
