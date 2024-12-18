// Licensed to the Biwen.Settings.OC under one or more agreements.
// The Biwen.Settings.OC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace Biwen.Settings.OC
{


    [Feature("Biwen.Settings.OC")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {

            using var sp = services.BuildServiceProvider().CreateScope();
            //var shell = sp.ServiceProvider.GetRequiredService<IShellConfiguration>();
            var settings = sp.ServiceProvider.GetRequiredService<ShellSettings>();

            var jsonPath = settings.ShellConfiguration.GetValue("Biwen.Settings:JsonFilePath", "systemsetting.json");
            var projectId = settings.ShellConfiguration.GetValue("Biwen.Settings:ProjectId", "Default");

            services.AddBiwenSettings(options =>
            {

                options.ProjectId = $"{settings.TenantId}-{projectId}";
                options.Title = "Biwen.Settings";
                options.PermissionValidator = async (ctx) =>
                {
                    //判断是否有权限访问设置页面
                    return await ctx.RequestServices
                    .GetService<IAuthorizationService>()
                    .AuthorizeAsync(ctx.User, Permissions.ManageSettings);
                };
                options.EditorOptions.EditorOnclick = "return confirm('Are You Sure!?');";
                options.EditorOptions.EdtiorConfirmButtonText = "Submit";
                options.EditorOptions.EditorEditButtonText = "Edit";
                options.EditorOptions.ShouldPagenation = true;
                //开启AutoFluentValidation
                options.AutoFluentValidationOption.Enable = true;

                //支持缓存提供者,默认不使用缓存
                //您也可以使用Biwen.Settings提供内存缓存:Biwen.Settings.Caching.MemoryCacheProvider
                //options.UseCacheOfNull();
                options.UseCacheOfMemory();

                //使用JsonStore
                options.UseStoreOfJsonFile(options =>
                {
                    options.FormatJson = true;
                    options.JsonPath = $"App_Data/{settings.TenantId}-{jsonPath}";
                });
            });
            //权限
            services.AddScoped<IPermissionProvider, Permissions>();
            //菜单
            services.AddScoped<INavigationProvider, AdminMenu>();
            //控制器
            services.AddScoped<Biwen.Settings.Controllers.SettingController>();

        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {

            var settingOption = serviceProvider.GetRequiredService<IOptions<SettingOptions>>();
            if (settingOption.Value.EditorOptions.ShouldPagenation)
            {
                //添加嵌入式资源
                var embeddedFileProvider = new EmbeddedFileProvider(typeof(ISetting).Assembly, "Biwen.Settings");

                builder.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = embeddedFileProvider,
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
                    }
                });
            }

            var settings = serviceProvider.GetRequiredService<ShellSettings>();
            var enlable = settings.ShellConfiguration.GetValue("Biwen.Settings:WebApi:Enlable", true);
            var routePrefix = settings.ShellConfiguration.GetValue("Biwen.Settings:WebApi:RoutePrefix", "biwensettings/api");

            //"WebApi": {
            //    "Enlable": true,
            //    "RoutePrefix": "biwensettings/api"
            //}

            if (enlable)
            {
                Console.WriteLine($"Biwen.Settings.WebApi: {routePrefix} Started!");
                routes.MapBiwenSettingApi(routePrefix!);
            }

            routes.MapAreaControllerRoute(
                   name: "settingRouteIndex",
                   areaName: "Biwen.Settings",
                   pattern: settingOption.Value.Route,
                   defaults: new { controller = "Setting", action = "Index" });

            routes.MapAreaControllerRoute(
                   name: "settingRouteEdit",
                   areaName: "Biwen.Settings",
                   pattern: "biwen/settings/setting/edit/{id}",
                   defaults: new { controller = "Setting", action = "Edit" });
        }
    }

}
