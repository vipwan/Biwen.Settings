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

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context == null)
        {
            return;
        }
        var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<SettingOptions>>();
        var isValid = options!.Value.PermissionValidator.Invoke(context.HttpContext);
        if (isValid)
        {
            base.OnActionExecuting(context);
        }
        else
        {
            context.Result = new UnauthorizedResult();
        }
    }
}