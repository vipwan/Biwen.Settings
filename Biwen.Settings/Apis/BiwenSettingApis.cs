// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:26:00 BiwenSettingApis.cs

using Biwen.Settings.Caching;
using Biwen.Settings.Encryption;
using Biwen.Settings.EndpointNotify;
using Biwen.Settings.Mvc;
using Biwen.Settings.SettingStores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Dynamic;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // 命名空间与文件夹结构不匹配

public static class BiwenSettingApis
{

    private static string RootNamespace => typeof(ISetting).Namespace!;

    /// <summary>
    /// 注册BiwenSetting Api
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="apiPrefix"></param>
    /// <param name="mapNotifyEndpoint">是否配置Settings变更消费者</param>
    /// <returns></returns>
    internal static RouteGroupBuilder MapBiwenSettingApi(
        this IEndpointRouteBuilder endpoint,
        string apiPrefix = "biwensetting/api",
        bool mapNotifyEndpoint = false)
    {
        //group
        var group = endpoint.MapGroup(apiPrefix);

        //tags
        group.WithTags(RootNamespace);

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
            var notifyEndpoint = endpoint.MapPost(Consts.EndpointUrl, NotifyAsync);

            //DTO
            notifyEndpoint.Accepts<NofityDto>(contentType: "application/json");
            notifyEndpoint.WithTags(RootNamespace);

#if !DEBUG
            notifyEndpoint.ExcludeFromDescription();//排除在Swagger文档中
#endif
        }
        return group;
    }


    #region invokes

    static Results<NotFound, JsonHttpResult<IEnumerable<SettingDto>>> GetAll(
        [FromServices] ISettingStore settingStore,
        [FromServices] IEncryptionProvider encryptionProvider)
    {
        var all = settingStore.GetAllSettings();
        return TypedResults.Json(all.Select(x => x.MapperToDto(encryptionProvider)));
    }

    static Results<NotFound, JsonHttpResult<SettingDto>> GetById(
        [FromServices] ISettingStore settingStore,
        [FromServices] IEncryptionProvider encryptionProvider,
        [FromRoute] string id)
    {
        if (string.IsNullOrEmpty(id)) return TypedResults.NotFound();
        var setting = settingStore.GetSetting(id);
        return setting == null ? TypedResults.NotFound() : TypedResults.Json(setting.MapperToDto(encryptionProvider));
    }

    static async Task<Results<BadRequest, Ok<object>>> SetByIdAsync(
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
            //忽略大小写.因为传输的对象可能是驼峰
            var key = dto!.Keys.FirstOrDefault(x => x.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(key))
                continue;
            //当前类型必须能转换String
            if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                continue;

            var strValue = dto[key];
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
        var settingContext = ctx.HttpContext!.RequestServices.GetRequiredService<IAsyncContext<SettingRecord>>();
        var saveSettingService = ctx.HttpContext!.RequestServices.GetRequiredService<SaveSettingService>();

        settingContext.Set(new SettingRecord(type!, setting));
        await saveSettingService.SaveSettingAsync();

        return TypedResults.Ok(setting);
    }

    static async Task<Results<Ok, BadRequest>> NotifyAsync(
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