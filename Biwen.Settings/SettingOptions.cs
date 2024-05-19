using Biwen.Settings.Caching;
using Biwen.Settings.Encryption;
using Microsoft.AspNetCore.Http;

namespace Biwen.Settings
{
    public class SettingOptions
    {
        /// <summary>
        /// 权限验证器,如果返回false则不允许访问设置页面
        /// </summary>
        public Func<HttpContext, bool> PermissionValidator { get; set; } = new Func<HttpContext, bool>(context => true);
        /// <summary>
        /// 管理页面路由路径
        /// </summary>
        public string Route { get; set; } = "system/setting";
        /// <summary>
        /// Layout布局
        /// </summary>
        [Obsolete("Layout后续不在使用,请添加Areas/Biwen.Settings/Views/Setting/_Layout.cshtml 覆盖", false)]
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
        public EditorOptions EditorOption { get; set; } = new();

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
        public (Type? ManagerType, object? Options) SettingManager { get; private set; } = (null, null);


        /// <summary>
        /// 使用的缓存提供者
        /// </summary>
        /// <param name="provider"></param>
        public void UseCache<T>() where T : class, ICacheProvider
        {
            CacheProvider = typeof(T);
        }

        /// <summary>
        /// 使用SettingManager
        /// </summary>
        /// <typeparam name="T">ISettingManager</typeparam>
        /// <typeparam name="V">扩展信息</typeparam>
        /// <param name="extend"></param>
        public void UseSettingManager<T, V>(V extend) where T : class, ISettingManager
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
        public NotifyOptions NotifyOption { get; set; } = new();

        /// <summary>
        /// 变更通知选项
        /// </summary>
        public class NotifyOptions
        {
            /// <summary>
            /// 是否启用当前服务为通知的生产者
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
    }

}