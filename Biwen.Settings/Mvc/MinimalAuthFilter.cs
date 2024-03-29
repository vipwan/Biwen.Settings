﻿using Microsoft.AspNetCore.Http;

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
            var flag = options.PermissionValidator(context.HttpContext);
            if (!flag)
            {
                return Results.Unauthorized();
            }
            return await next(context);
        }
    }
}