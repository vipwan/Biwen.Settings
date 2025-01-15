// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:29:52 SettingBase.cs

using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace Biwen.Settings;

/// <summary>
/// 继承此类的配置项，将会被自动注册到配置中心,如果需要验证,请继承自ValidationSettingBase<T>
/// </summary>
public abstract class SettingBase<T> : ISetting,
#region fit for IOptions
    IValidateOptions<T>, IConfigureOptions<T>, IPostConfigureOptions<T> where T : class, ISetting, new()
    #endregion
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

    #region IOptions 兼容

    public void Configure(T options)
    {
        using var scope = ServiceRegistration.ServiceProvider.CreateScope();
        var settingStore = scope.ServiceProvider.GetRequiredService<ISettingStore>();
        var setting = settingStore.GetAsync<T>().GetAwaiter().GetResult();
        //将配置项的值赋值给options
        //options = setting;
        scope.ServiceProvider.GetService<IConfiguration>()?.Bind(typeof(T).Name, options);
    }

    public virtual ValidateOptionsResult Validate(string? name, T options)
    {
        return ValidateOptionsResult.Success;
    }

    public void PostConfigure(string? name, T options)
    {
        using var scope = ServiceRegistration.ServiceProvider.CreateScope();
        var settingStore = scope.ServiceProvider.GetRequiredService<ISettingStore>();
        var setting = settingStore.GetAsync<T>().GetAwaiter().GetResult();
        //将配置项的值赋值给options
        //options = setting;
        scope.ServiceProvider.GetService<IConfiguration>()?.Bind(typeof(T).Name, options);
    }

    #endregion

}