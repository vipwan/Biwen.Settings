
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Dynamic;

namespace Microsoft.AspNetCore.Builder
{

#if NET7_0_OR_GREATER 

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
            var group = endpoint.MapGroup(routePrefix).AddEndpointFilter(async (ctx, next) =>
                    {
                        var options = ctx.HttpContext.RequestServices.GetService<IOptions<SettingOptions>>()!.Value!;
                        var flag = options.HasPermission(ctx.HttpContext);
                        if (!flag)
                        {
                            return Results.Unauthorized();
                        }
                        var result = await next(ctx);
                        return result;
                    });
            //all
            group.MapGet("all", (ISettingManager settingManager) =>
            {
                var all = settingManager.GetAllSettings();
                return Results.Json(all);
            });
            //get
            group.MapGet("get/{id}", (ISettingManager settingManager, [FromRoute] string id) =>
            {
                if (string.IsNullOrEmpty(id)) return Results.NotFound();

                var setting = settingManager.GetSetting(id);
                if (setting == null)
                {
                    return Results.NotFound();
                }
                return Results.Json(setting);
            });
            //set/{id}
            group.MapPost("set/{id}", async (
                HttpRequest request,
                ISettingManager settingManager,
                IOptions<SettingOptions> options,
                [FromRoute] string id) =>
            {
                if (string.IsNullOrEmpty(id)) return Results.NotFound();
                var type = ASS.InAllRequiredAssemblies.FirstOrDefault(x => x.FullName == id);
                if (type == null) return Results.NotFound();
                //json object ->mapper
                var dto = await request.ReadFromJsonAsync(type);
                if (dto == null) return Results.NotFound();

                //验证DTO
                Func<MethodInfo?, object, (bool, dynamic?)> Valid = (md, validator) =>
                {
                    //验证不通过的情况
                    if (md!.Invoke(validator, new[] { dto! }) is ValidationResult result && !result!.IsValid)
                    {
                        var dic = new List<(string, string)>();
                        foreach (var item in result.Errors)
                        {
                            dic.Add((item.PropertyName, item.ErrorMessage));
                        }
                        return (false, dic.ToJsonObj());
                    }
                    return (true, null);
                };

                //验证DTO
                if (options.Value.AutoFluentValidationOption.Enable)
                {
                    //存在验证器的情况
                    var validator = request.HttpContext!.RequestServices.GetService(
                        serviceType: typeof(IValidator<>).MakeGenericType(type));
                    if (validator != null)
                    {
                        var md = validator.GetType().GetMethods().First(
                            x => !x.IsGenericMethod && x.Name == nameof(IValidator.Validate));
                        var reslut = Valid(md, validator);
                        if (!reslut.Item1)
                        {
                            return Results.BadRequest(reslut.Item2);
                        }
                    }
                    //继承至ValidationSettingBase<T>的情况
                    if (type.BaseType!.IsConstructedGenericType && type.BaseType!.GenericTypeArguments.Any(x => x == type))
                    {
                        var x = dto as ISettingValidator ?? throw new BiwenException($"ISettingValidator is Null!");
                        var md = x.RealValidator.GetType().GetMethods().First(
                            x => !x.IsGenericMethod && x.Name == nameof(IValidator.Validate));
                        //验证不通过的情况
                        var result = Valid(md, x.RealValidator);
                        if (!result.Item1)
                        {
                            return Results.BadRequest(result.Item2);
                        }
                    }
                }

                //保存
                var mdSave = settingManager.GetType().GetMethod(nameof(ISettingManager.Save))!.MakeGenericMethod(type);
                mdSave.Invoke(settingManager, new[] { dto! });
                return Results.Ok();
            });
            return group;
        }

        private static dynamic ToJsonObj(this List<(string, string)> values)
        {
            dynamic obj = new ExpandoObject();
            foreach (var item in values)
            {
                //如果重复,则忽略
                ((IDictionary<string, object>)obj).TryAdd(item.Item1, item.Item2);
            }
            return obj;
        }

    }

#endif
}