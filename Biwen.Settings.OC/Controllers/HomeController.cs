using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace Biwen.Settings.OC.Controllers
{
    [Admin]
    public class HomeController : Controller
    {
        private readonly Biwen.Settings.Controllers.SettingController _settingController;

        public HomeController(Biwen.Settings.Controllers.SettingController settingController)
        {
            _settingController = settingController;
        }
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
        public IActionResult Edit(string id, IFormCollection form)
        {
            return _settingController.Edit(id, form);
        }
    }
}