// Licensed to the Biwen.Settings.Redis under one or more agreements.
// The Biwen.Settings.Redis licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings.Redis Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-12-25 14:09:40 RedisClientOptions.cs

namespace Biwen.Settings.Redis;

/// <summary>
/// CsRedis 配置
/// </summary>
public class RedisClientOptions
{
    /// <summary>
    /// 链接字符串,see:https://github.com/2881099/csredis
    /// default:127.0.0.1:6379
    /// </summary>
    public string RedisConnString { get; set; } = "127.0.0.1:6379";

    /// <summary>
    /// 集群
    /// </summary>
    public string[]? Clusters { get; set; }

}
