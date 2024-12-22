// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:29:44 ServiceRegistration.cs

using Biwen.Settings.Caching;
using Biwen.Settings.Encryption;
using Biwen.Settings.EndpointNotify;
using Biwen.Settings.SettingStores;
using Biwen.Settings.SettingStores.EFCore;
using Biwen.Settings.SettingStores.JsonFile;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Biwen.Settings;

public static class ServiceRegistration
{
    /// <summary>
    /// ServiceProvider
    /// </summary>
    internal static IServiceProvider ServiceProvider = null!;

    /// <summary>
    /// Add BiwenSettings
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <param name="configuration">注册到Configuration</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddBiwenSettings(this IServiceCollection services,
        Action<SettingOptions> options = null!)
    {
        //AsyncStateHttpContext
        services.AddAsyncStateHttpContext();
        services.AddControllersWithViews();

        #region Localization

        services.AddLocalization(options =>
        {
            options.ResourcesPath = "Localization.resources";
        });

        services.AddScoped<BiwenSettingsLocalizer>();

        #endregion

        services.AddOptions<SettingOptions>().Configure(x => { options?.Invoke(x); });

        var currentOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SettingOptions>>().Value;

        #region 注入缓存

        services.AddMemoryCache();
        var cacheTypeProvider = currentOptions.CacheProvider;
        services.AddSingleton(typeof(ICacheProvider), cacheTypeProvider!);

        #endregion

        #region 注入Encryption

        var encryptionProvider = currentOptions.EncryptionProvider;
        services.AddScoped(typeof(IEncryptionProvider), encryptionProvider);

        #endregion

        #region 注入SettingStore

        if (currentOptions.SettingStore.StoreType == typeof(EFCoreSettingStore<>))
        {
            ArgumentNullException.ThrowIfNull(currentOptions.SettingStore.Options, "EFCoreStoreOptions need set!");

            services.AddOptions<EFCoreStoreOptions>().Configure(x =>
            {
                (currentOptions.SettingStore.Options as Action<EFCoreStoreOptions>)?.Invoke(x);
            });
        }
        else if (currentOptions.SettingStore.StoreType == typeof(JsonFileSettingStore))
        {
            services.AddOptions<JsonFileStoreOptions>().Configure(x =>
            {
                (currentOptions.SettingStore.Options as Action<JsonFileStoreOptions>)?.Invoke(x);
            });

            //文件变更通知,自动启动:
            services.AddActivatedSingleton<FileChangeNotifier>();
        }
        else if (currentOptions.SettingStore.StoreType == null)
        {
            throw new BiwenException("Require ISettingStore!");
        }

        services.AddScoped(currentOptions.SettingStore.StoreType!);
        services.AddScoped<ISettingStore, SettingStoreDecorator>(sp =>
        {
            var store = sp.GetRequiredService(currentOptions.SettingStore.StoreType!);
            return new SettingStoreDecorator((ISettingStore)store, sp);
        });

        //注入Lazy<ISettingStore>:
        services.AddScoped(sp => new Lazy<ISettingStore>(sp.GetRequiredService<ISettingStore>));

        //SaveSettingService
        services.AddScoped<SaveSettingService>();

        #endregion

        #region INotify

        foreach (var notify in Notifys)
        {
            //存在一个订阅者订阅多个事件的情况:
            var baseTypes = notify.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == InterfaceINotify).ToArray();
            foreach (var baseType in baseTypes)
            {
                services.AddScoped(baseType, notify);
            }
        }

        //Medirator
        services.AddScoped<IMedirator, Medirator>();

        #endregion

        //注册验证器
        if (currentOptions.AutoFluentValidationOption.Enable)
        {
            //注册验证器
            services.AddFluentValidationAutoValidation();
        }

        var settings = ASS.InAllRequiredAssemblies.ThatInherit<ISetting>().Where(x => x.IsClass && !x.IsAbstract).ToArray();

        //消费者通知服务
        services.AddScoped<NotifyServices>();

        //注册ISetting
        foreach (var settingType in settings)
        {
            //Self DI
            services.AddScoped(settingType, sp => GetSetting(settingType, sp));
        };

        //sp
        ServiceProvider = services.BuildServiceProvider();

        // 初始化设置
        using var scope = ServiceProvider.CreateScope();
        settings.AsParallel().ForAll(settingType =>
        {
            try { var setting = scope.ServiceProvider.GetRequiredService(settingType) as ISetting; }
            catch
            {
                //todo:避免数据库Migration阶段编译报错
            }
        });

        return services;
    }

    #region internal

    static readonly Lock _lock = new();//锁
    static readonly Type InterfaceINotify = typeof(INotify<>);

    static IEnumerable<Type> _notifys = null!;

    static bool IsToGenericInterface(Type type, Type baseInterface)
    {
        if (type == null) return false;
        if (baseInterface == null) return false;

        return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == baseInterface);
    }

    static IEnumerable<Type> Notifys
    {
        get
        {
            lock (_lock)
                return _notifys ??= ASS.InAllRequiredAssemblies.Where(x =>
                !x.IsAbstract && x.IsClass && x.IsPublic && IsToGenericInterface(x, InterfaceINotify));
        }
    }

    static readonly ConcurrentDictionary<Type, MethodInfo> _cachedMethods = new();

    static object GetSetting(Type x, IServiceProvider sp)
    {
        var settingStore = sp.GetRequiredService<ISettingStore>();

        var md = _cachedMethods.GetOrAdd(x, (type) =>
        {
            MethodInfo methodLoad = settingStore.GetType().GetMethod(nameof(settingStore.Get))!;
            MethodInfo generic = methodLoad.MakeGenericMethod(type);
            return generic;
        });
        return md!.Invoke(settingStore, null)!;
    }

    #endregion

    /// <summary>
    /// Use BiwenSettings
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseBiwenSettings(this WebApplication app)
    {
        var settingOptions = app.Services.GetRequiredService<IOptions<SettingOptions>>();
        if (settingOptions.Value.EditorOptions.ShouldPagenation)
        {
            //添加嵌入式资源
            var embeddedFileProvider = new EmbeddedFileProvider(typeof(ISetting).Assembly, "Biwen.Settings");

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = embeddedFileProvider,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
                }
            });
        }

        app.UseRequestLocalization();

        app.MapAreaControllerRoute(
               name: "settingRouteIndex",
               areaName: "Biwen.Settings",
               pattern: settingOptions.Value.Route,
               defaults: new { controller = "Setting", action = "Index" });

        app.MapAreaControllerRoute(
               name: "settingRouteEdit",
               areaName: "Biwen.Settings",
               pattern: "biwen/settings/setting/edit/{id}",
               defaults: new { controller = "Setting", action = "Edit" });

        // WebApi
        var route = app.MapBiwenSettingApi(settingOptions.Value.ApiPrefix, settingOptions.Value.MapNotifyEndpoint);
        settingOptions.Value.ApiConventionBuilder?.Invoke(route);
        return app;
    }
}