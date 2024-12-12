// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:29:58 EFCoreStoreOptions.cs

using static Biwen.Settings.SettingOptions;

namespace Biwen.Settings.SettingManagers.EFCore;

public class EFCoreStoreOptions
{
    /// <summary>
    /// 加密的内容配置项
    /// </summary>
    public EncryptionOptions EncryptionOptions { get; set; } = new();

}