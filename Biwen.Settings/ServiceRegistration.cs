using Microsoft.Extensions.DependencyInjection;

namespace Biwen.Settings
{
    using Biwen.Settings.EntityFramework;
    using FluentValidation;
    using FluentValidation.AspNetCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System.Reflection;

    public static class ServiceRegistration
    {
        /// <summary>
        /// Add BiwenSettings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dbContextType"></param>
        /// <param name="options"></param>
        /// <param name="autoValidation">是否FluentValidation自动验证</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddBiwenSettings(this IServiceCollection services, Type dbContextType,
            Action<SettingOptions>? options = null, bool autoValidation = true)
        {
            if (dbContextType == null)
                throw new ArgumentNullException(nameof(dbContextType));

            services.AddHttpContextAccessor();
            services.AddControllersWithViews();
            services.AddMemoryCache();
            services.AddTransient((IServiceProvider p) => (p.GetRequiredService(dbContextType) as IBiwenSettingsDbContext)!);
            services.AddOptions<SettingOptions>().Configure(x => { options?.Invoke(x); });
            services.AddScoped<ISettingManager, SettingManager>();

            services.AddTransient((IServiceProvider p) =>
            {
                return (p.GetRequiredService(dbContextType) as IBiwenSettingsDbContext)!;
            });

            services.AddOptions<SettingOptions>().Configure(x => { options?.Invoke(x); });

            services.AddScoped<ISettingManager, SettingManager>();


            var settings = TypeFinder.FindTypes.InAllAssemblies.ThatInherit(typeof(ISetting)).Where(x => x.IsClass && !x.IsAbstract).ToList();

            settings.ForEach(x =>
            {
                services.AddScoped(x, sp =>
                {
                    var settingManager = sp.GetRequiredService<ISettingManager>();
                    var cache = sp.GetRequiredService<IMemoryCache>();

                    //使用缓存避免重复反射
                    var md = cache.GetOrCreate($"GenericMethod_{x.FullName}", entry =>
                    {
                        MethodInfo methodLoad = settingManager.GetType().GetMethod("Get")!;
                        MethodInfo generic = methodLoad.MakeGenericMethod(x);
                        return generic;
                    });
                    return md!.Invoke(settingManager, null)!;
                });

                // 初始化设置
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
                using var scope = services.BuildServiceProvider()!.CreateScope();
                try { var setting = scope.ServiceProvider.GetRequiredService(x) as ISetting; }
                catch
                {
                    //todo:避免数据库Migration阶段编译报错
                }
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'

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