using Microsoft.AspNetCore.Http;

namespace Biwen.Settings
{

    public class SettingOptions
    {
        /// <summary>
        /// 默认无需验证
        /// </summary>
        public Func<HttpContext, bool> Valider { get; set; } = new Func<HttpContext, bool>(context => true);
        /// <summary>
        /// 默认setting
        /// </summary>
        public string Route { get; set; } = "system/setting";

        public string Layout { get; set; } = "_Layout.cshtml";

        public string Title { get; set; } = "设置中心";
        /// <summary>
        /// 项目标识 用于区分不同的项目,比如:日志系统,文件系统;或者环境,比如:开发环境,测试环境,生产环境
        /// </summary>
        public string ProjectId { get; set; } = "default";

    }
}
