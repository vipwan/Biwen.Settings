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
    }
}
