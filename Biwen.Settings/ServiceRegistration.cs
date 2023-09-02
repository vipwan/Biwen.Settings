using Biwen.Settings.Caching;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

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

            services.AddOptions<SettingOptions>().Configure(x => { options?.Invoke(x); });

            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var currentOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SettingOptions>>();
            #region 注入缓存

            var cacheTypeProvider = currentOptions.Value.CacheProvider;

            //Memory缓存提供者
            //services.AddMemoryCache();
            //services.AddScoped<ICacheProvider, MemoryCacheProvider>();
            //空缓存提供者
            //services.AddScoped<ICacheProvider, NullCacheProvider>();

            if (cacheTypeProvider == typeof(MemoryCacheProvider))
            {
                services.AddScoped<ICacheProvider, MemoryCacheProvider>();
            }
            else if (cacheTypeProvider == typeof(NullCacheProvider))
            {
                services.AddScoped<ICacheProvider, NullCacheProvider>();
            }
            else
            {
                services.Scan(scan =>
                {
                    scan.FromAssemblies(allAssemblies).AddClasses(x =>
                    {
                        x.Where(a => { return a == cacheTypeProvider; });
                    })
                    .AsImplementedInterfaces() //实现基于他的接口
                    .WithScopedLifetime();  //Scoped
                });
            }

            #endregion

            if (currentOptions.Value.SettingManager.Item1 == typeof(EntityFrameworkCoreSettingManager))
            {
                if (currentOptions.Value.SettingManager.Item2 == null)
                    throw new BiwenException("Require IBiwenSettingsDbContext ExtendType!");

                services.AddTransient((IServiceProvider p) =>
                (p.GetRequiredService((Type)currentOptions.Value.SettingManager.Item2) as IBiwenSettingsDbContext)!);
                services.AddScoped<ISettingManager, EntityFrameworkCoreSettingManager>();
            }
            else
            {
                if (currentOptions.Value.SettingManager.Item1 == null)
                    throw new BiwenException("Require ISettingManager!");

                services.Scan(scan =>
                {
                    scan.FromAssemblies(allAssemblies).AddClasses(x =>
                    {
                        x.AssignableTo(typeof(ISettingManager)).Where(a =>
                        {
                            return a == currentOptions.Value.SettingManager.Item1!;
                        });
                    })
                    .AsImplementedInterfaces() //实现基于他的接口
                    .WithScopedLifetime();  //Scoped
                });
            }


            if (currentOptions.Value.AutoFluentValidationOption.Enable)
            {
                //注册验证器
                services.AddFluentValidationAutoValidation();
                services.Scan(scan =>
                {
                    scan.FromAssemblies(allAssemblies.
                        Where(x => !(x.FullName!.Contains("FluentValidation")))).AddClasses(x =>
                        {
                            x.AssignableTo(typeof(IValidator<>));//来自指定的接口
                            //必须是类,且当前Class不是泛型类.排除ValidationSettingBase<T>,且不为抽象类
                            x.Where(a => { return a.IsClass && !a.IsAbstract && !a.IsGenericTypeDefinition; });
                        })
                    .AsImplementedInterfaces(x => x.IsGenericType) //实现基于他的接口
                    .WithTransientLifetime();  //AddTransient
                });
            }

            var settings = FindTypes.InAssemblies(allAssemblies).ThatInherit(
                typeof(ISetting)).Where(x => x.IsClass && !x.IsAbstract).ToList();

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
                var assembly = typeof(ISetting).Assembly;
                var embeddedFileProvider = new EmbeddedFileProvider(assembly, "Biwen.Settings");

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = embeddedFileProvider,
                });
            }

            app.MapControllerRoute(
                   name: "settingRouteIndex",
                   pattern: settingOption.Value.Route,
                   defaults: new { controller = "Setting", action = "Index" });

            app.MapControllerRoute(
                   name: "settingRouteEdit",
                   pattern: "biwen/settings/setting/edit/{id}",
                   defaults: new { controller = "Setting", action = "Edit" });

            return app;
        }

    }
}