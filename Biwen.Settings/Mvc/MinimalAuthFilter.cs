using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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
            var flag = options.HasPermission(context.HttpContext);
            if (!flag)
            {
                return Results.Unauthorized();
            }
            return await next(context);
        }
    }
}