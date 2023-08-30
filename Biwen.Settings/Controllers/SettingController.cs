using Biwen.Settings.Domains;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json.Nodes;

namespace Biwen.Settings.Controllers
{
    public class SettingController : Controller
    {
        private readonly IOptions<SettingOptions> _options;
        private readonly ISettingManager _settingManager;

        public SettingController(IOptions<SettingOptions> options, ISettingManager settingManager)
        {
            _options = options;
            _settingManager = settingManager;
        }

        //[HttpGet("qwertyuiopasdfghjklzxcvbnm/setting")]
        public IActionResult Index()
        {
            var isValid = _options.Value.Valider.Invoke(Request.HttpContext);

            if (!isValid)
                return Unauthorized();

            var all = _settingManager.GetAllSettings();

            //移除的或者无效的配置 需要排除
            var settings = all.Where(
                s => TypeFinder.FindTypes.InAllAssemblies.Any(x => x.FullName == s.SettingName));

            ViewBag.Settings = settings;

            return View();
        }

        public IActionResult Edit(string id)
        {
            var isValid = _options.Value.Valider.Invoke(Request.HttpContext);
            if (!isValid)
                return Unauthorized();

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var setting = _settingManager.GetAllSettings().FirstOrDefault(x => x.SettingName == id);
            if (setting == null)
                return NotFound();

            var type = TypeFinder.FindTypes.InAllAssemblies.FirstOrDefault(x => x.FullName == setting.SettingName);
            if (type == null)
                return NotFound();
            List<Tuple<string, string?, string?>> SettingValues = new();
            var json = JsonObject.Parse(setting.SettingContent!)!;
            type.GetProperties().Where(x =>
                x.Name != nameof(Setting.SettingName) &&
                x.Name != nameof(Setting.ProjectId) &&
                x.Name != nameof(Setting.Order) &&
                x.Name != nameof(Setting.Description) &&
                //x.Name != nameof(Setting.Version) &&
                x.Name != nameof(Setting.SettingContent) &&
                x.Name != nameof(Setting.LastModificationTime)).ToList().ForEach(x =>
                {
                    var instanceSetting = Request.HttpContext.RequestServices.GetService(type);
                    var desc = x.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

                    var value = json[x.Name];
                    if (value != null)
                    {
                        SettingValues.Add(new Tuple<string, string?, string?>(x.Name, desc?.Description, value.ToString()));
                    }
                    else
                    {
                        var propertyValue = type.GetProperty(x.Name)!.GetValue(instanceSetting);
                        SettingValues.Add(new Tuple<string, string?, string?>(x.Name, desc?.Description, propertyValue == null ? string.Empty : propertyValue.ToString()));
                    }
                });

            ViewBag.Setting = setting;
            ViewBag.SettingValues = SettingValues;

            return View();
        }


        [HttpPost]
        public IActionResult Edit(string id, IFormCollection form)
        {
            var isValid = _options.Value.Valider.Invoke(Request.HttpContext);
            if (!isValid)
                return Unauthorized();

            var type = TypeFinder.FindTypes.InAllAssemblies.FirstOrDefault(x => x.FullName == id);
            if (type == null)
                return NotFound();

            var sitting = Request.HttpContext.RequestServices.GetService(type)!;

            foreach (string? item in form.Keys)
            {
                PropertyInfo prop = null!;
                try
                {
                    prop = sitting.GetType().GetProperty(item)!;
                }
                catch
                {
                    //todo:
                }

                //SetMethod 判断
                if (prop?.SetMethod == null)
                {
                    continue;
                }

                if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(sitting, Convert.ToBoolean(form[item].ToString())); continue;
                }
                if (prop.PropertyType == typeof(int))
                {
                    prop.SetValue(sitting, Convert.ToInt32(form[item].ToString())); continue;
                }
                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(sitting, form[item].ToString()); continue;
                }
                if (prop.PropertyType == typeof(Guid))
                {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    prop.SetValue(sitting, new Guid(form[item].ToString())); continue;
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                }
                if (prop.PropertyType == typeof(DateTime))
                {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    prop.SetValue(sitting, DateTime.Parse(form[item].ToString())); continue;
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                }
                if (prop.PropertyType == typeof(double))
                {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    prop.SetValue(sitting, double.Parse(form[item].ToString())); continue;
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                }
                if (prop.PropertyType == typeof(decimal))
                {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    prop.SetValue(sitting, decimal.Parse(form[item].ToString())); continue;
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                }
                if (prop.PropertyType == typeof(byte))
                {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    prop.SetValue(sitting, byte.Parse(form[item].ToString())); continue;
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                }
                if (prop.PropertyType == typeof(char))
                {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    prop.SetValue(sitting, char.Parse(form[item].ToString())); continue;
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                }
                if (prop.PropertyType == typeof(float))
                {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    prop.SetValue(sitting, float.Parse(form[item].ToString())); continue;
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                }
                if (prop.PropertyType == typeof(long))
                {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                    prop.SetValue(sitting, long.Parse(form[item].ToString())); continue;
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                }
                //prop.SetValue(sitting, form[item]);
            }

            MethodInfo methodLoad = _settingManager.GetType().GetMethod("Save")!;
            MethodInfo generic = methodLoad.MakeGenericMethod(type);
            generic.Invoke(_settingManager, new object[] { sitting });
            //_settingManager.Save(sitting as SettingBase);
            return RedirectToAction("Edit", new { id });

        }

    }
}
