// Licensed to the Biwen.Settings.Redis under one or more agreements.
// The Biwen.Settings.Redis licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings.Redis Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-12-25 14:17:05 ServiceCollectionExtensions.cs

using CSRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Biwen.Settings.Redis;

public static class ServiceRegistration
{
    /// <summary>
    /// 添加CSRedisClient
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection AddCsRedisClient(this IServiceCollection services, Action<RedisClientOptions> options)
    {
        services.Configure(options);
        services.AddActivatedSingleton(static services =>
        {
            var redisClientOptions = services.GetRequiredService<IOptions<RedisClientOptions>>().Value;
            return new CSRedisClient(redisClientOptions.RedisConnString, redisClientOptions.Clusters ?? []);
        });

        return services;
    }
}