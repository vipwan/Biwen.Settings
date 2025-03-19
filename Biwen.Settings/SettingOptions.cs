// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:30:52 SettingOptions.cs

using Biwen.Settings.Caching;
using Biwen.Settings.Encryption;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Biwen.Settings;

public class SettingOptions
{
    /// <summary>
    /// 权限验证器,如果返回false则不允许访问设置页面,默认:允许任意访问
    /// </summary>
    public Func<HttpContext, Task<bool>> PermissionValidator { get; set; } = _ => Task.FromResult(true);
    /// <summary>
    /// 管理页面路由路径,默认:system/setting
    /// </summary>
    public string Route { get; set; } = "system/setting";

    /// <summary>
    /// 管理页面标题,默认:设置中心
    /// </summary>
    public string Title { get; set; } = "设置中心";
    /// <summary>
    /// 项目标识 用于区分不同的项目,比如:日志系统,文件系统;或者环境,比如:开发环境,测试环境,生产环境,默认:default
    /// </summary>
    public string ProjectId { get; set; } = "default";

    /// <summary>
    /// 编辑器选项
    /// </summary>
    public EditorOptions EditorOptions { get; set; } = new();

    /// <summary>
    /// 自动Setting验证器配置
    /// </summary>
    public AutoFluentValidationOptions AutoFluentValidationOption { get; set; } = new();

    /// <summary>
    /// 默认不使用任何缓存
    /// 不支持直接调用,请使用UseCache方法,默认:NullCacheProvider
    /// </summary>
    public Type CacheProvider { get; private set; } = typeof(NullCacheProvider);


    /// <summary>
    /// 缓存时间,默认:86400秒
    /// </summary>
    internal int CacheTime { get; private set; } = 86400;


    /// <summary>
    /// SettingStore
    /// </summary>
    internal (Type? StoreType, object? Options) SettingStore { get; private set; } = (null, null);


    /// <summary>
    /// 使用的缓存提供者
    /// </summary>
    /// <param name="provider"></param>
    public void UseCache<T>(int cacheTime = 86400) where T : class, ICacheProvider
    {
        CacheTime = cacheTime;//缓存时间
        CacheProvider = typeof(T);
    }

    /// <summary>
    /// 使用SettingStore
    /// </summary>
    /// <typeparam name="T">ISettingStore</typeparam>
    /// <typeparam name="V">扩展信息</typeparam>
    /// <param name="extend"></param>
    public void UseSettingStore<T, V>(V extend) where T : class, ISettingStore
    {
        SettingStore = (typeof(T), extend);
    }

    /// <summary>
    /// 自动Setting验证器配置
    /// </summary>
    public class AutoFluentValidationOptions
    {
        /// <summary>
        /// 默认开启
        /// </summary>
        public bool Enable { get; set; } = true;

    }

    /// <summary>
    /// 默认不使用任何缓存
    /// 不支持直接调用,请使用UseCache方法
    /// </summary>
    public Type EncryptionProvider { get; private set; } = typeof(EmptyEncryptionProvider);

    public void UseEncryption<T>() where T : class, IEncryptionProvider
    {
        EncryptionProvider = typeof(T);
    }

    /// <summary>
    /// 内容是否加密的选项,
    /// 请注意,如果启用了加密,请务必保证所有的配置项都是加密的,否则会导致配置项无法正常读取
    /// </summary>
    public class EncryptionOptions
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; } = false;
    }

    /// <summary>
    /// 通知选项
    /// </summary>
    public NotifyOptions NotifyOptions { get; set; } = new();

    /// <summary>
    /// WebApi相关配置
    /// </summary>
    public ApiOptions ApiOptions { get; set; } = new();
}

/// <summary>
/// 通知选项
/// </summary>
public class NotifyOptions
{
    /// <summary>
    /// 是否启用当前服务为通知的生产者,默认:false
    /// </summary>
    public bool IsNotifyEnable { get; set; } = false;

    /// <summary>
    /// 通知的密钥,用于验证通知的合法性
    /// 请注意如果作为消费者,即使Enable=false,也必须设置Secret,否则会导致无法接收到通知
    /// </summary>
    public string Secret { get; set; } = "Biwen.Settings.Notify";

    /// <summary>
    /// 消费者的地址集合
    /// 如:http://localhost:5000
    /// </summary>
    public string[] EndpointHosts { get; set; } = [];

}

/// <summary>
/// EditorOptions
/// </summary>
public class EditorOptions
{
    /// <summary>
    /// js Onclick代码,默认:return confirm('确定要修改吗!?如果格式错误将使系统异常!');
    /// </summary>
    public string? EditorOnclick { get; set; } = "return confirm('确定要修改吗!?如果格式错误将使系统异常!');";
    /// <summary>
    /// 按钮确认文本,默认:确认修改
    /// </summary>
    public string? EdtiorConfirmButtonText { get; set; } = "确认修改";

    /// <summary>
    /// 按钮文本,默认:编辑
    /// </summary>
    public string? EditorEditButtonText { get; set; } = "编辑";

    /// <summary>
    /// 是否分页展示,注意Layout必须引用JQuery,默认:true
    /// </summary>
    public bool ShouldPagenation { get; set; } = true;

    /// <summary>
    /// 分页大小,默认:10
    /// </summary>
    public int PageSize { get; set; } = 10;

}


/// <summary>
/// ApiOptions
/// </summary>
public class ApiOptions
{
    /// <summary>
    /// 是否开启Api,默认:true
    /// </summary>
    public bool ApiEnabled { get; set; } = true;


    /// <summary>
    /// Minimal Api前缀,默认:biwensetting/api
    /// </summary>
    public string ApiPrefix { get; set; } = "biwensetting/api";

    /// <summary>
    /// Api EndpointConventionBuilder,默认:null
    /// </summary>
    public Action<IEndpointConventionBuilder>? ApiConventionBuilder { get; set; } = null;

    /// <summary>
    /// 是否开启提醒的Notify端点,默认:false
    /// </summary>
    public bool MapNotifyEndpoint { get; set; } = false;
}
