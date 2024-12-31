// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:29:28 MinimalAuthFilter.cs

using Microsoft.AspNetCore.Http;

namespace Biwen.Settings.Mvc;

/// <summary>
/// Filter for MinimalApi auth
/// </summary>
internal class MinimalAuthFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var options = context.HttpContext.RequestServices.GetService<IOptions<SettingOptions>>()!.Value!;
        if (await options.PermissionValidator(context.HttpContext))
        {
            return await next(context);
        }
        return Results.Unauthorized();
    }
}