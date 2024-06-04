using Biwen.Settings.Caching;
using Biwen.Settings.Encryption;
using Biwen.Settings.EndpointNotify;
using Biwen.Settings.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Dynamic;

namespace Microsoft.AspNetCore.Builder
{
    public static class BiwenSettingApis
    {
        /// <summary>
        /// 注册BiwenSetting Api
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="routePrefix"></param>
        /// <param name="mapNotifyEndpoint">是否配置Settings变更消费者</param>
        /// <returns></returns>
        public static RouteGroupBuilder MapBiwenSettingApi(
            this IEndpointRouteBuilder endpoint,
            string routePrefix = "biwensetting/api",
            bool mapNotifyEndpoint = false)
        {
            //group
            var group = endpoint.MapGroup(routePrefix);
            //auth
            group.AddEndpointFilter<MinimalAuthFilter>();
            //all
            group.MapGet("all", GetAll).Produces<List<SettingDto>>();
            //get
            group.MapGet("get/{id}", GetById).Produces<SettingDto>();
            //set/{id}
            group.MapPost("set/{id}", SetByIdAsync).Accepts<ExpandoObject>(
                contentType: "application/json-patch+json").AddEndpointFilter<ValidDtoFilter>();

            if (mapNotifyEndpoint)
            {
                //notify
                var notifyEndpoint = endpoint.MapPost(Consts.EndpointUrl, Notify);

                //DTO
                notifyEndpoint.Accepts<NofityDto>(contentType: "application/json");
                notifyEndpoint.WithTags("Notify");
#if !DEBUG
                notifyEndpoint.ExcludeFromDescription();//排除在Swagger文档中
#endif
            }
            return group;
        }


        #region invokes

        static Results<NotFound, JsonHttpResult<IEnumerable<SettingDto>>> GetAll(
            [FromServices] ISettingManager settingManager,
            [FromServices] IEncryptionProvider encryptionProvider)
        {
            var all = settingManager.GetAllSettings();
            return TypedResults.Json(all.Select(x => x.MapperToDto(encryptionProvider)));
        }

        static Results<NotFound, JsonHttpResult<SettingDto>> GetById(
            [FromServices] ISettingManager settingManager,
            [FromServices] IEncryptionProvider encryptionProvider,
            [FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id)) return TypedResults.NotFound();
            var setting = settingManager.GetSetting(id);
            return setting == null ? TypedResults.NotFound() : TypedResults.Json(setting.MapperToDto(encryptionProvider));
        }

        static async Task<Results<BadRequest, Ok<object>>> SetByIdAsync(
            [FromServices] ISettingManager settingManager,
            [FromServices] IOptions<SettingOptions> options,
            [FromServices] IHttpContextAccessor ctx,
            [FromRoute] string id)
        {
            //ValidDtoFilter Before
            var type = ASS.InAllRequiredAssemblies.First(x => x.FullName == id);
            //json ->dto
            if ((await ctx.HttpContext!.Request.ReadFromJsonAsync<ExpandoObject>()) is not IDictionary<string, object> dto)
            {
                return TypedResults.BadRequest();
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
            mdSave.Invoke(settingManager, [setting!]);
            return TypedResults.Ok(setting);
        }

        static async Task<Results<Ok, BadRequest>> Notify(
            [FromServices] IOptions<SettingOptions> options,
            [FromServices] ICacheProvider cacheProvider,
            //[FromServices] IHttpContextAccessor ctx,
            [FromRoute] string secret,
            [FromBody] NofityDto dto)
        {

            if (secret != options.Value.NotifyOptions.Secret)
            {
                return TypedResults.BadRequest();
            }
            //var dto = await ctx.HttpContext!.Request.ReadFromJsonAsync<NofityDto>();
            if (dto == null) return TypedResults.BadRequest();
            await cacheProvider.RemoveAsync(string.Format(Consts.CacheKeyFormat, dto.SettingType, options.Value.ProjectId));

            Console.WriteLine($"消费了配置变更:{dto.SettingType} and Clear cache");
            return TypedResults.Ok();
        }

        #endregion


        /// <summary>
        /// Dto
        /// </summary>
        /// <param name="SettingType"></param>
        /// <param name="SettingName"></param>
        /// <param name="Description"></param>
        /// <param name="SettingContent"></param>
        /// <param name="LastModificationTime"></param>
        internal record SettingDto(string SettingType, string SettingName, string? Description, string? SettingContent, DateTime LastModificationTime);

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
        private static dynamic ToExpandoObject(this List<(string Prop, string Val)> values)
        {
            dynamic obj = new ExpandoObject();
            foreach (var (Prop, Val) in values)
            {
                //如果重复,则忽略
                ((IDictionary<string, object>)obj).TryAdd(Prop, Val);
            }
            return obj;
        }

        #endregion

    }
}