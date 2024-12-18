// Licensed to the Biwen.Settings.OC under one or more agreements.
// The Biwen.Settings.OC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Biwen.Settings.OC;


/// <summary>
/// Filter for auth
/// </summary>
internal class AuthAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context == null)
            return;
        var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<SettingOptions>>();
        if (await options!.Value.PermissionValidator.Invoke(context.HttpContext))
        {
            await next();
        }
        else
        {
            context.Result = new UnauthorizedResult();
        }
    }
}