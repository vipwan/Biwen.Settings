using Biwen.Settings.Caching;
using Biwen.Settings.SettingManagers.EFCore;
using Biwen.Settings.SettingManagers.JsonStore;
using Biwen.Settings.TestWebUI.Settings;
using Microsoft.AspNetCore.Http;

namespace Biwen.Settings
{

    public class SettingOptions
    {
        /// <summary>
        /// 权限验证器,如果返回false则不允许访问设置页面
        /// </summary>
        public Func<HttpContext, bool> Valider { get; set; } = new Func<HttpContext, bool>(context => true);
        /// <summary>
        /// 管理页面路由路径
        /// </summary>
        public string Route { get; set; } = "system/setting";
        /// <summary>
        /// Layout布局
        /// </summary>
        public string Layout { get; set; } = "_Layout.cshtml";
        /// <summary>
        /// 管理页面标题
        /// </summary>
        public string Title { get; set; } = "设置中心";
        /// <summary>
        /// 项目标识 用于区分不同的项目,比如:日志系统,文件系统;或者环境,比如:开发环境,测试环境,生产环境
        /// </summary>
        public string ProjectId { get; set; } = "default";

        /// <summary>
        /// 编辑器选项
        /// </summary>
        public EditorOptions EditorOption { get; set; } = new EditorOptions();

        /// <summary>
        /// 自动Setting验证器配置
        /// </summary>
        public AutoFluentValidationOptions AutoFluentValidationOption { get; set; } = new();


        /// <summary>
        /// 默认不使用任何缓存
        /// 不支持直接调用,请使用UseCache方法
        /// </summary>
        public Type CacheProvider { get; private set; } = typeof(NullCacheProvider);

        /// <summary>
        /// 默认使用EntityFrameworkCore持久化配置项
        /// </summary>
        public (Type?, object?) SettingManager { get; private set; } = (null, null);


        /// <summary>
        /// 使用的缓存提供者
        /// </summary>
        /// <param name="provider"></param>
        public void UseCache<T>() where T : ICacheProvider
        {
            CacheProvider = typeof(T);
        }


        /// <summary>
        /// 使用SettingManager
        /// </summary>
        /// <typeparam name="T">ISettingManager</typeparam>
        /// <typeparam name="V">扩展信息</typeparam>
        /// <param name="extend"></param>
        public void UseSettingManager<T, V>(V extend) where T : ISettingManager
        {
            SettingManager = (typeof(T), extend);
        }

        public class EditorOptions
        {
            /// <summary>
            /// js Onclick代码
            /// </summary>
            public string? EditorOnclick { get; set; } = "return confirm('确定要修改吗!?如果格式错误将使系统异常!');";
            /// <summary>
            /// 按钮文本
            /// </summary>
            public string? EdtiorConfirmButtonText { get; set; } = "确认修改";

            public string? EditorEditButtonText { get; set; } = "编辑";

            /// <summary>
            /// 是否分页展示,注意Layout必须引用JQuery
            /// </summary>
            public bool ShouldPagenation { get; set; } = true;

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



    }


    public static class SettingOptionsExtension
    {

        /// <summary>
        /// 使用内存缓存
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>

        public static SettingOptions UseCacheOfMemory(this SettingOptions options)
        {
            options.UseCache<MemoryCacheProvider>();
            return options;
        }

        /// <summary>
        /// Default不使用缓存
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static SettingOptions UseCacheOfNull(this SettingOptions options)
        {
            options.UseCache<NullCacheProvider>();
            return options;
        }

        /// <summary>
        /// 使用EntityFmeworkCore持久化配置项
        /// </summary>
        /// <param name="options"></param>
        /// <param name="contextType"></param>
        /// <returns></returns>
        public static SettingOptions UseSettingManagerOfEFCore(this SettingOptions options, Type dbContextType)
        {
            options.UseSettingManager<EntityFrameworkCoreSettingManager, Type>(dbContextType);
            return options;
        }


        public static SettingOptions UserSettingManagerOfJsonStore(this SettingOptions options, Action<JsonStoreOptions>? storePptions = null)
        {
            options.UseSettingManager<JsonStoreSettingManager, Action<JsonStoreOptions>?>(storePptions);
            return options;
        }
    }

}