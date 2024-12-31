// Licensed to the Biwen.Settings.OC under one or more agreements.
// The Biwen.Settings.OC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Mvc.Core.Utilities;

namespace Biwen.Settings.OC.Controllers;

[Admin]
public class HomeController(IOptions<AdminOptions> adminOptions, Biwen.Settings.Controllers.SettingController settingController) : Controller
{
    private readonly Biwen.Settings.Controllers.SettingController _settingController = settingController;
    private readonly IOptions<AdminOptions> _adminOptions = adminOptions;

    [Auth]
    public IActionResult Index()
    {
        return RedirectToAction("Setting");
    }

    [Auth]
    public IActionResult Setting()
    {
        return _settingController.Index();
    }
    [Auth]
    public IActionResult Edit(string id)
    {
        return _settingController.Edit(id);
    }

    [Auth]
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Edit(string id, IFormCollection form)
    {
        //routes.MapAreaControllerRoute(
        //name: "settingRouteIndex",
        //areaName: "Biwen.Settings",
        //pattern: settingOption.Value.Route,
        //defaults: new { controller = "Setting", action = "Index" });

        var redirectUrl = $"{Url.GetBaseUrl()}/{_adminOptions.Value.AdminUrlPrefix}/Biwen.Settings.OC/Home/Setting";
        return _settingController.Edit(id, form, redirectUrl);
    }
}