﻿
using System.Text.Json;
using Biwen.Settings.Domains;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Biwen.Settings
{
    using Biwen.Settings.EntityFramework;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    internal sealed class SettingManager : ISettingManager
    {
        private readonly IBiwenSettingsDbContext _db;
        private readonly ILogger<SettingManager> _logger;
        private readonly IMemoryCache _cache;
        private readonly IOptions<SettingOptions> _options;

        private const string CacheKeyFormat = "SettingManager_{0}";


        public SettingManager(
            IBiwenSettingsDbContext db,
            ILogger<SettingManager> logger,
            IMemoryCache cache,
            IOptions<SettingOptions> options)
        {
            _db = db;
            _logger = logger;
            _cache = cache;
            _options = options;
        }

        public T Get<T>() where T : ISetting, new()
        {

            return _cache.GetOrCreate(string.Format(CacheKeyFormat, typeof(T).FullName), entry =>
            {

                var @default = new T();
                var setting = _db.Settings.FirstOrDefault(x => x.ProjectId == _options.Value.ProjectId && x.SettingName == @default.SettingName);

                if (setting != null)
                {
                    @default = JsonSerializer.Deserialize<T>(setting.SettingContent!)!;
                }
                else
                {
                    var desc = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                    _db.Settings.Add(new Setting
                    {
                        ProjectId = _options.Value.ProjectId,
                        SettingName = @default.SettingName!,
                        Description = desc != null ? ((DescriptionAttribute)desc).Description : null,
                        Order = @default.Order,
                        LastModificationTime = DateTime.Now,
                        SettingContent = JsonSerializer.Serialize(@default)
                    });
                    (_db as DbContext)!.SaveChanges();
                }

                if (@default == null)
                {
                    _logger.LogError(message: $"{typeof(T).FullName} 未找到配置");
                    throw new Exception($"{typeof(T).FullName} 未找到配置");
                }
                //不可为空
                return @default;

            })!;
        }

        public void Save<T>(T setting) where T : ISetting
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            var settingName = setting.SettingName!;
            var settingContent = JsonSerializer.Serialize(setting);
            var settingEntity = _db.Settings.FirstOrDefault(x => x.ProjectId == _options.Value.ProjectId && x.SettingName == settingName);
            if (settingEntity != null)
            {
                settingEntity.SettingContent = settingContent;
                settingEntity.LastModificationTime = DateTime.Now;
            }
            else
            {
                var desc = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                _db.Settings.Add(new Setting
                {
                    ProjectId = _options.Value.ProjectId,
                    SettingName = settingName,
                    Description = desc != null ? ((DescriptionAttribute)desc).Description : null,
                    Order = setting.Order,
                    LastModificationTime = DateTime.Now,
                    SettingContent = settingContent
                });
            }
            (_db as DbContext)!.SaveChanges();
            _cache.Remove(string.Format(CacheKeyFormat, typeof(T).FullName));

            _logger.LogInformation(message: $"保存配置 {settingName},{settingContent}");

        }

        public List<Setting> GetAllSettings()
        {
            return _db.Settings.Where(x => x.ProjectId == _options.Value.ProjectId).OrderBy(x => x.Order).ThenBy(x => x.SettingName).ToList();
        }
    }
}