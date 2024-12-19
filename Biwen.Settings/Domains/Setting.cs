// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:27:13 Setting.cs

using Microsoft.EntityFrameworkCore;

namespace Biwen.Settings.Domains;


[PrimaryKey("ProjectId", "SettingType")]//双主键
public class Setting
{
    /// <summary>
    /// 项目标识 用于区分不同的项目,比如:日志系统,文件系统;或者环境,比如:开发环境,测试环境,生产环境
    /// </summary>
    public string ProjectId { get; set; } = null!;

    /// <summary>
    ///  配置类型.
    /// </summary>
    [MSDA.MaxLength(500)]
    public string SettingType { get; set; } = null!;

    /// <summary>
    /// 配置名称
    /// </summary>
    [MSDA.MaxLength(500)]
    public string SettingName { get; set; } = null!;


    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    [DefaultValue(1000)]
    public int Order { get; set; } = 1000;
    /// <summary>
    /// JSON存储
    /// </summary>
    [DefaultValue("{}")]
    [MSDA.ConcurrencyCheck]
    public string? SettingContent { get; set; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastModificationTime { get; set; } = DateTime.Now;


    //特定数据库不支持.为保证功能可用,调整为SettingContent检测

    /*  
            /// <summary>
            /// 乐观并发锁,注意SQLite文件数据库不支持该特性
            /// </summary>
            [Timestamp]
            public byte[] Version { get; set; } = null!;
    */
}