// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:29:33 SettingAuthorizeAttribute.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Biwen.Settings.Mvc;

/// <summary>
/// Filter for auth
/// </summary>
internal class SettingAuthorizeAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context == null)
            return;

        var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<SettingOptions>>();
        if (await options.Value.PermissionValidator.Invoke(context.HttpContext))
        {
            await next();
        }
        else
        {
            context.Result = new UnauthorizedResult();
        }
    }
}