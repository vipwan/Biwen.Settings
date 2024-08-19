using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Biwen.Settings.Mvc;

/// <summary>
/// Filter for auth
/// </summary>
internal class SettingAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context == null)
        {
            return;
        }
        var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<SettingOptions>>();
        if (options.Value.PermissionValidator.Invoke(context.HttpContext))
        {
            base.OnActionExecuting(context);
        }
        else
        {
            context.Result = new UnauthorizedResult();
        }
    }
}