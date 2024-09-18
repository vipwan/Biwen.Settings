// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:28:52 BiwenSettingsLocalizer.cs

namespace Biwen.Settings;

/// <summary>
/// IStringLocalizer<T> Or BiwenSettingsLocalizer.T
/// </summary>
public class BiwenSettingsLocalizer(IStringLocalizer<UI> ui)
{
    /// <summary>
    /// UI Localization
    /// </summary>
    public IStringLocalizer<UI> UI { get; private set; } = ui;
}