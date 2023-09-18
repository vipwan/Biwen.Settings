using Biwen.Settings.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Biwen.Settings.Controllers
{

    [Area("Biwen.Settings")]
    public class SettingController : Controller
    {
        private readonly ISettingManager _settingManager;
        private readonly IOptions<SettingOptions> _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SettingController(
            ISettingManager settingManager,
            IOptions<SettingOptions> options,
            IHttpContextAccessor httpContextAccessor)
        {
            _settingManager = settingManager;
            _options = options;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        //[HttpGet("qwertyuiopasdfghjklzxcvbnm/setting")]
        [Auth]
        public IActionResult Index()
        {
            var all = _settingManager.GetAllSettings();

            //移除的或者无效的配置 需要排除
            var settings = all.Where(
                s => ASS.InAllRequiredAssemblies.Any(x => x.FullName == s.SettingType));

            ViewBag.Settings = settings;

            return View();
        }

        [Auth]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var setting = _settingManager.GetSetting(id);
            if (setting == null)
                return NotFound();

            var type = ASS.InAllRequiredAssemblies.FirstOrDefault(x => x.FullName == setting.SettingType);
            if (type == null)
                return NotFound();

            ViewBag.Setting = setting;
            ViewBag.SettingValues = SettingValues(setting);

            return View();
        }

        [NonAction]
        List<(string, string?, string?)> SettingValues(Setting setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            var type = ASS.InAllRequiredAssemblies.FirstOrDefault(x => x.FullName == setting.SettingType)
                ?? throw new ArgumentNullException(nameof(setting));

            List<(string, string?, string?)> SettingValues = new();
            var json = JsonNode.Parse(setting.SettingContent!)!;
            type.GetProperties().Where(x =>
                !x.GetCustomAttributes<JsonIgnoreAttribute>().Any() &&
                x.CanWrite && x.CanRead &&
                x.Name != nameof(Setting.SettingType) &&
                x.Name != nameof(Setting.ProjectId) &&
                x.Name != nameof(Setting.SettingName) &&
                x.Name != nameof(Setting.Order) &&
                x.Name != nameof(Setting.Description) &&
                //x.Name != nameof(Setting.Version) &&
                x.Name != nameof(Setting.SettingContent) &&
                x.Name != nameof(Setting.LastModificationTime)).ToList().ForEach(x =>
                {
                    var instanceSetting = _httpContextAccessor!.HttpContext!.RequestServices.GetService(type);
                    var desc = x.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

                    var value = json[x.Name];
                    if (value != null)
                    {
                        SettingValues.Add((x.Name, desc?.Description, value.ToString()));
                    }
                    else
                    {
                        var propertyValue = type.GetProperty(x.Name)!.GetValue(instanceSetting);
                        SettingValues.Add((x.Name, desc?.Description, propertyValue == null ? string.Empty : propertyValue.ToString()));
                    }
                });

            return SettingValues;
        }

        [Auth, HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(string id, IFormCollection form)
        {
            var type = ASS.InAllRequiredAssemblies.FirstOrDefault(x => x.FullName == id);
            if (type == null)
                return NotFound();

            var setting = _httpContextAccessor!.HttpContext!.RequestServices.GetService(type)!;

            foreach (string? item in form.Keys)
            {
                PropertyInfo prop = null!;
                try
                {
                    prop = setting.GetType().GetProperty(item)!;
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
                //当前类型必须能转换String
                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;
                //当前类型必须能转换传递的参数值
                var strValue = form[item].ToString();
                if (!TypeDescriptor.GetConverter(prop.PropertyType).IsValid(strValue))
                    continue;
                //转换
                var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(strValue);
                //赋值
                prop.SetValue(setting, value);

            }

            //验证DTO
            Func<MethodInfo?, object, bool> Valid = (md, validator) =>
            {
                //验证不通过的情况
                if (md!.Invoke(validator, new[] { setting! }) is ValidationResult result && !result!.IsValid)
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                    }
                    var domainSetting = _settingManager.GetSetting(id);
                    ViewBag.Setting = domainSetting!;
                    ViewBag.SettingValues = SettingValues(domainSetting!);
                    //验证不通过
                    return false;
                }
                return true;
            };

            if (_options.Value.AutoFluentValidationOption.Enable)
            {
                //存在验证器的情况
                var validator = _httpContextAccessor!.HttpContext!.RequestServices.GetService(serviceType: typeof(IValidator<>).MakeGenericType(type));
                if (validator != null)
                {
                    var md = validator.GetType().GetMethods().First(x => !x.IsGenericMethod && x.Name == nameof(IValidator.Validate));

                    if (!Valid(md, validator))
                    {
                        return View();
                    }
                }

                //继承至ValidationSettingBase<T>的情况
                if (type.BaseType!.IsConstructedGenericType && type.BaseType!.GenericTypeArguments.Any(x => x == type))
                {
                    var x = setting as ISettingValidator ?? throw new BiwenException($"ISettingValidator is Null!");
                    var md = x.RealValidator.GetType().GetMethods().First(x => !x.IsGenericMethod && x.Name == nameof(IValidator.Validate));
                    //验证不通过的情况
                    if (!Valid(md, x.RealValidator))
                    {
                        return View();
                    }
                }
            }

            var mdSave = _settingManager.GetType().GetMethod(nameof(ISettingManager.Save))!.MakeGenericMethod(type);
            mdSave.Invoke(_settingManager, new[] { setting });
            //return RedirectToAction("Edit", new { id });
            return RedirectToAction("Index");
        }

    }
}