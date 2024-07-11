using Biwen.Settings.Caching;
using Biwen.Settings.Encryption;
using Biwen.Settings.EndpointNotify;
using Biwen.Settings.SettingManagers;
using Biwen.Settings.SettingManagers.EFCore;
using Biwen.Settings.SettingManagers.JsonStore;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Biwen.Settings
{
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

            #region 注入SettingManager

            if (currentOptions.SettingManager.ManagerType == typeof(EntityFrameworkCoreSettingManager))
            {
                if (currentOptions.SettingManager.Options == null)
                    throw new BiwenException("EFCoreStoreOptions need set!");

                services.AddOptions<EFCoreStoreOptions>().Configure(x =>
                {
                    (currentOptions.SettingManager.Options as Action<EFCoreStoreOptions>)?.Invoke(x);
                });
            }
            else if (currentOptions.SettingManager.ManagerType == typeof(JsonStoreSettingManager))
            {
                services.AddOptions<JsonStoreOptions>().Configure(x =>
                {
                    (currentOptions.SettingManager.Options as Action<JsonStoreOptions>)?.Invoke(x);
                });

                //文件变更通知,自动启动:
                services.AddActivatedSingleton<FileChangeNotifier>();
            }
            else
            {
                if (currentOptions.SettingManager.ManagerType == null)
                    throw new BiwenException("Require ISettingManager!");
            }

            services.AddScoped(currentOptions.SettingManager.ManagerType!);
            services.AddScoped<ISettingManager, SettingManagerDecorator>(sp =>
            {
                var manager = sp.GetRequiredService(currentOptions.SettingManager.ManagerType!);
                return new SettingManagerDecorator((ISettingManager)manager, sp);
            });

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

                foreach (var validator in Validators)
                {
                    services.AddTransient(InterfaceIValidator, validator);
                }
            }

            var settings = ASS.InAllRequiredAssemblies.ThatInherit(
                typeof(ISetting)).Where(x => x.IsClass && !x.IsAbstract).ToList();

            //消费者通知服务
            services.AddScoped<NotifyServices>();

            //sp
            ServiceProvider = services.BuildServiceProvider();

            //注册ISetting
            settings.ForEach(x =>
            {
                //Self DI
                services.AddScoped(x, sp => GetSetting(x, sp));

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

        #region internal

        static readonly object _lock = new();//锁
        static readonly Type InterfaceINotify = typeof(INotify<>);
        static readonly Type InterfaceIValidator = typeof(IValidator<>);

        static IEnumerable<Type> _notifys = null!;
        static IEnumerable<Type> _validators = null!;

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

        static IEnumerable<Type> Validators
        {
            get
            {
                lock (_lock)
                    return _validators ??= ASS.InAllRequiredAssemblies.Where(x =>
                    !x.IsAbstract && x.IsClass && x.IsPublic && IsToGenericInterface(x, InterfaceIValidator));
            }
        }

        static object GetSetting(Type x, IServiceProvider sp)
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
        }

        #endregion

        /// <summary>
        /// Use BiwenSettings
        /// </summary>
        /// <param name="app"></param>
        /// <param name="routePrefix"></param>
        /// <param name="mapNotifyEndpoint">是否配置Settings变更消费者</param>
        /// <param name="builder">需要对MinimalApi更多的扩展操作</param>
        /// <returns></returns>
        public static IApplicationBuilder UseBiwenSettings(
            this WebApplication app,
            string routePrefix = "biwensetting/api",
            bool mapNotifyEndpoint = false,
            Action<IEndpointConventionBuilder>? builder = null
            )
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
            var route = app.MapBiwenSettingApi(routePrefix, mapNotifyEndpoint);
            builder?.Invoke(route);
            return app;
        }
    }
}