using Microsoft.AspNetCore.Http;

namespace Biwen.Settings.Mvc
{
    /// <summary>
    /// Filter for MinimalApi auth
    /// </summary>
    internal class MinimalAuthFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var options = context.HttpContext.RequestServices.GetService<IOptions<SettingOptions>>()!.Value!;
            if (options.PermissionValidator(context.HttpContext))
            {
                return await next(context);
            }
            return Results.Unauthorized();
        }
    }
}