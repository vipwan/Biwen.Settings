// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:28:40 ISetting.cs

namespace Biwen.Settings;

/// <summary>
/// 配置接口,使用请继承自:<see cref="SettingBase{T}"/> or <see cref="ValidationSettingBase{T}"/>
/// </summary>
public interface ISetting
{
    string? SettingName { get; }
    int Order { get; }

}

internal interface ISettingValidator
{
    //object RealValidator { get; }

    /// <summary>
    /// 验证当前的Request
    /// </summary>
    /// <returns></returns>
    ValidationResult Validate();

}