using Biwen.Settings.Caching;
using Biwen.Settings.SettingManagers.EFCore;
using Biwen.Settings.SettingManagers.JsonStore;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Biwen.Settings
{
    public static class ServiceRegistration
    {


        /// <summary>
        /// Add BiwenSettings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddBiwenSettings(this IServiceCollection services,
            Action<SettingOptions> options = null!)
        {
            services.AddHttpContextAccessor();
            services.AddControllersWithViews();
            services.AddMemoryCache();

            #region Localization

            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Localization.resources";
            });

            services.AddScoped<BiwenSettingsLocalizer>();

            #endregion

            services.AddOptions<SettingOptions>().Configure(x => { options?.Invoke(x); });

            var currentOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SettingOptions>>();

            #region 注入缓存

            var cacheTypeProvider = currentOptions.Value.CacheProvider;
            services.AddScoped(typeof(ICacheProvider), cacheTypeProvider!);

            #endregion

            #region 注入SettingManager

            if (currentOptions.Value.SettingManager.Item1 == typeof(EntityFrameworkCoreSettingManager))
            {
                if (currentOptions.Value.SettingManager.Item2 == null)
                    throw new BiwenException("EFCoreStoreOptions need set!");

                services.AddOptions<EFCoreStoreOptions>().Configure(x =>
                {
                    (currentOptions.Value.SettingManager.Item2 as Action<EFCoreStoreOptions>)?.Invoke(x);
                });
                //services.AddScoped<ISettingManager, EntityFrameworkCoreSettingManager>();
            }
            else if (currentOptions.Value.SettingManager.Item1 == typeof(JsonStoreSettingManager))
            {
                services.AddOptions<JsonStoreOptions>().Configure(x =>
                {
                    (currentOptions.Value.SettingManager.Item2 as Action<JsonStoreOptions>)?.Invoke(x);
                });
                //services.AddScoped<ISettingManager, JsonStoreSettingManager>();
            }
            else
            {
                if (currentOptions.Value.SettingManager.Item1 == null) throw new BiwenException("Require ISettingManager!");

                //services.AddScoped(typeof(ISettingManager), currentOptions.Value.SettingManager.Item1!);
            }

            services.AddScoped(typeof(ISettingManager), currentOptions.Value.SettingManager.Item1!);

            #endregion

            #region INotify

            services.Scan(scan =>
            {
                scan.FromAssemblies(ASS.AllRequiredAssemblies).AddClasses(x =>
                {
                    x.AssignableTo(typeof(INotify<>)).Where(a =>
                    {
                        return a.IsClass && !a.IsAbstract;
                    });
                })
                .AsImplementedInterfaces() //实现基于他的接口
                .WithScopedLifetime();  //Scoped
            });

            //Medirator
            services.AddScoped<IMedirator, Medirator>();

            #endregion

            //装饰ISettingManager
            services.Decorate<ISettingManager>((inner, provider) => new SettingManagerDecorator(inner, provider));

            //注册验证器
            if (currentOptions.Value.AutoFluentValidationOption.Enable)
            {
                //注册验证器
                services.AddFluentValidationAutoValidation();
                services.Scan(scan =>
                {
                    scan.FromAssemblies(ASS.AllRequiredAssemblies).AddClasses(x =>
                        {
                            x.AssignableTo(typeof(IValidator<>));//来自指定的接口
                            //必须是类,且当前Class不是泛型类.排除ValidationSettingBase<T>,且不为抽象类
                            x.Where(a => { return a.IsClass && !a.IsAbstract && !a.IsGenericTypeDefinition; });
                        })
                    .AsImplementedInterfaces(x => x.IsGenericType) //实现基于他的接口
                    .WithTransientLifetime();  //AddTransient
                });
            }

            var settings = ASS.InAllRequiredAssemblies.ThatInherit(
                typeof(ISetting)).Where(x => x.IsClass && !x.IsAbstract).ToList();

            //注册ISetting
            settings.ForEach(x =>
            {
                services.AddScoped(x, sp =>
                {

                    var settingManager = sp.GetRequiredService<ISettingManager>();
                    var cache = sp.GetRequiredService<IMemoryCache>();

                    //使用缓存避免重复反射
                    var md = cache.GetOrCreate($"GenericMethod_{x.FullName}", entry =>
                    {
                        MethodInfo methodLoad = settingManager.GetType().GetMethod(nameof(settingManager.Get))!;
                        MethodInfo generic = methodLoad.MakeGenericMethod(x);
                        return generic;
                    });
                    return md!.Invoke(settingManager, null)!;
                });

                // 初始化设置
                using var scope = services.BuildServiceProvider()!.CreateScope();
                try { var setting = scope.ServiceProvider.GetRequiredService(x) as ISetting; }
                catch
                {
                    //todo:避免数据库Migration阶段编译报错
                }
            });

            return services;
        }


        /// <summary>
        /// Use BiwenSettings
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseBiwenSettings(this WebApplication app)
        {
            var settingOption = app.Services.GetRequiredService<IOptions<SettingOptions>>();
            if (settingOption.Value.EditorOption.ShouldPagenation)
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
                   pattern: settingOption.Value.Route,
                   defaults: new { controller = "Setting", action = "Index" });

            app.MapAreaControllerRoute(
                   name: "settingRouteEdit",
                   areaName: "Biwen.Settings",
                   pattern: "biwen/settings/setting/edit/{id}",
                   defaults: new { controller = "Setting", action = "Edit" });

            return app;
        }

    }
}