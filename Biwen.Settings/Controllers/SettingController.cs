// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:27:05 SettingController.cs

using Biwen.Settings.Encryption;
using Biwen.Settings.Mvc;
using Biwen.Settings.SettingStores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Biwen.Settings.Controllers;

/// <summary>
/// SettingViewModel
/// </summary>
public readonly record struct SettingViewModel(string Name, string? Description, string? Value);

[Area("Biwen.Settings")]
public class SettingController(
    Lazy<ISettingStore> settingStore,
    IOptions<SettingOptions> options,
    IEncryptionProvider encryptionProvider,
    IHttpContextAccessor httpContextAccessor) : Controller
{
    private readonly Lazy<ISettingStore> _settingStore = settingStore;
    private readonly IOptions<SettingOptions> _options = options;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly IEncryptionProvider _encryptionProvider = encryptionProvider;

    private readonly Lazy<IAsyncContext<SettingRecord>> _settingRecord =
        new(() => httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IAsyncContext<SettingRecord>>());

    private readonly Lazy<SaveSettingService> _saveSettingService =
        new(() => httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<SaveSettingService>());


    //[HttpGet("qwertyuiopasdfghjklzxcvbnm/setting")]
    [SettingAuthorize]
    public IActionResult Index()
    {
        var all = _settingStore.Value.GetAllSettings();

        //移除的或者无效的配置 需要排除
        var settings = all.Where(
            s => ASS.InAllRequiredAssemblies.Any(x => x.FullName == s.SettingType));

        ViewBag.Settings = settings;

        return View();
    }

    [SettingAuthorize]
    public IActionResult Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var setting = _settingStore.Value.GetSetting(id);
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
    List<SettingViewModel> SettingValues(Setting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        var type = ASS.InAllRequiredAssemblies.FirstOrDefault(x => x.FullName == setting.SettingType)
            ?? throw new ArgumentNullException(nameof(setting));

        List<SettingViewModel> SettingValues = [];

        var plainContent = _encryptionProvider.Decrypt(setting.SettingContent!);

        var json = JsonNode.Parse(plainContent)!;

        var instanceSetting = _httpContextAccessor!.HttpContext!.RequestServices.GetService(type);

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
                var desc = x.GetCustomAttribute<DescriptionAttribute>();
                var value = json[x.Name];

                var vm = new SettingViewModel(x.Name, desc?.Description, value?.ToString());

                if (value != null)
                {
                    SettingValues.Add(vm);
                }
                else
                {
                    var propertyValue = type.GetProperty(x.Name)!.GetValue(instanceSetting);
                    SettingValues.Add(vm with
                    {
                        Value = propertyValue == null ? string.Empty : propertyValue.ToString()
                    });
                }
            });

        return SettingValues;
    }

    [SettingAuthorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, IFormCollection form, string? redirectUrl = null)
    {
        var type = ASS.InAllRequiredAssemblies.FirstOrDefault(x => x.FullName == id);
        if (type == null)
            return NotFound();

        var setting = _httpContextAccessor!.HttpContext!.RequestServices.GetService(type)!;

        foreach (string item in form.Keys)
        {
            PropertyInfo prop = null!;
            try
            {
                prop = setting.GetType().GetProperty(item)!;
            }
            catch
            {
                continue;
            }

            //SetMethod 判断
            if (prop?.SetMethod == null)
                continue;

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

        if (_options.Value.AutoFluentValidationOption.Enable)
        {

            //继承至ValidationSettingBase<T>的情况
            if (type.BaseType!.IsConstructedGenericType && type.BaseType!.GenericTypeArguments.Any(x => x == type))
            {
                var x = setting as ISettingValidator ?? throw new BiwenException($"ISettingValidator is Null!");

                //验证不通过的情况
                var vResult = x.Validate();
                if (!vResult.IsValid)
                {
                    foreach (var item in vResult.Errors)
                    {
                        ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                    }
                    var domainSetting = _settingStore.Value.GetSetting(id);
                    ViewBag.Setting = domainSetting!;
                    ViewBag.SettingValues = SettingValues(domainSetting!);
                    return View();
                }
            }

            //验证DTO
            bool Valid(object validator)
            {
                var md = validator.GetType().GetMethods().First(x => !x.IsGenericMethod && x.Name == nameof(IValidator.Validate));
                //验证不通过的情况
                if (md!.Invoke(validator, [setting!]) is ValidationResult result && !result!.IsValid)
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                    }
                    var domainSetting = _settingStore.Value.GetSetting(id);
                    ViewBag.Setting = domainSetting!;
                    ViewBag.SettingValues = SettingValues(domainSetting!);
                    //验证不通过
                    return false;
                }
                return true;
            }

            //存在验证器的情况
            var validator = _httpContextAccessor!.HttpContext!.RequestServices.GetService(serviceType: typeof(IValidator<>).MakeGenericType(type));
            if (validator != null)
            {
                if (!Valid(validator))
                {
                    return View();
                }
            }
        }

        //Save
        _settingRecord.Value.Set(new SettingRecord(type, setting));
        await _saveSettingService.Value.SaveSettingAsync();

        if (string.IsNullOrEmpty(redirectUrl))
        {
            return RedirectToAction("Index", new { area = "Biwen.Settings" });
        }
        return Redirect(redirectUrl);
    }
}