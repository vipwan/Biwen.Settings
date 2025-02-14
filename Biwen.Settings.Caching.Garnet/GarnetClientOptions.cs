// Licensed to the Biwen.Settings.Caching.Garnet under one or more agreements.
// The Biwen.Settings.Caching.Garnet licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Biwen.Settings.Caching.Garnet;

/// <summary>
/// GarnetClientOptions
/// </summary>
public class GarnetClientOptions
{
    /// <summary>
    /// ip地址,默认:127.0.0.1
    /// </summary>
    public string? Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// 端口,默认:3278
    /// </summary>
    public int Port { get; set; } = 3278;

    public string? UserName { get; set; } = null;

    public string? Password { get; set; } = null;
}