
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Biwen.Settings.Domains
{

    [PrimaryKey("ProjectId", "SettingName")]//双主键
    public class Setting
    {
        /// <summary>
        /// 项目标识 用于区分不同的项目,比如:日志系统,文件系统;或者环境,比如:开发环境,测试环境,生产环境
        /// </summary>
        public string ProjectId { get; set; } = null!;

        /// <summary>
        /// 配置名称
        /// </summary>
        [MaxLength(500)]
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
        [ConcurrencyCheck]
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
}