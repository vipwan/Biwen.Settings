// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:27:47 ServiceRegistration.cs

using Microsoft.Extensions.Configuration;

namespace Biwen.Settings.Extensions.Configuration;

public static class ServiceRegistration
{
    internal static IServiceCollection AddBiwenSettingConfiguration(this IServiceCollection services)
    {
        //ConfigurationMediratorDoneHandler
        services.AddActivatedSingleton<IMediratorDoneHandler, ConfigurationMediratorDoneHandler>();
        return services;
    }

    /// <summary>
    /// 提供对IConfiguration,IOptions的支持
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="autoRefresh"></param>
    /// <returns></returns>
    public static ConfigurationManager AddBiwenSettingConfiguration(
        this ConfigurationManager manager, IServiceCollection serviceDescriptors, bool autoRefresh = true)
    {
        var sp = Settings.ServiceRegistration.ServiceProvider ?? throw new BiwenException("必须首先注册Biwen.Setting模块,请调用:services.AddBiwenSettings()");
        //添加订阅
        if (autoRefresh)
        {
            serviceDescriptors.AddBiwenSettingConfiguration();
        }
        IConfigurationBuilder configBuilder = manager;
        configBuilder.Add(new BiwenSettingConfigurationSource(autoRefresh));
        var settings = ASS.InAllRequiredAssemblies.ThatInherit<ISetting>().Where(x => x.IsClass && !x.IsAbstract).ToList();
        //注册ISetting
        settings.ForEach(x =>
        {
            //IOptions DI
            //manager?.GetSection(x.Name).Bind(GetSetting(x, sp));
            serviceDescriptors.ConfigureOptions(x);
        });

        //add IOptions
        sp = serviceDescriptors.BuildServiceProvider();

        return manager;
    }
}
