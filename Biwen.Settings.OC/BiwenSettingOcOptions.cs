using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.Settings.OC
{
    public class BiwenSettingOcOptions
    {
        public string JsonFilePath { get; set; } = "App_Data/Sites/Default/BiwenSettings.json";

        public string SettingUrl { get; set; } = "Admin/Biwen.Settings.OC/Home/Setting";

    }
}