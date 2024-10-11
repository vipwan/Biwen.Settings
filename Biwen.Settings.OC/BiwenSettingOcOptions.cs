// Licensed to the Biwen.Settings.OC under one or more agreements.
// The Biwen.Settings.OC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Biwen.Settings.OC;

public class BiwenSettingOcOptions
{
    public string JsonFilePath { get; set; } = "App_Data/Sites/Default/BiwenSettings.json";

    public string SettingUrl { get; set; } = "Admin/Biwen.Settings.OC/Home/Setting";

}