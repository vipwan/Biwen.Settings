
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Biwen.Settings
{
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

        /// <summary>
        /// 序列化配置项时的选项
        /// </summary>
        JsonSerializerOptions SerializerOptions = new()
        {
            IgnoreReadOnlyProperties = true,
            IgnoreReadOnlyFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

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
                        SettingContent = JsonSerializer.Serialize(@default, SerializerOptions)
                    });
                    (_db as DbContext)!.SaveChanges();
                }

                if (@default == null)
                {
                    _logger.LogError(message: "SettingType: {0} Not Found!", typeof(T).FullName);
                    throw new Exception($"SettingType: {typeof(T).FullName} Not Found!");
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
            var settingContent = JsonSerializer.Serialize(setting, SerializerOptions);
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

            _logger.LogInformation(message: "SaveSetting: {0},{1}", settingName, settingContent);
        }

        public List<Setting> GetAllSettings()
        {
            return _db.Settings.Where(x => x.ProjectId == _options.Value.ProjectId).OrderBy(x => x.Order).ThenBy(x => x.SettingName).ToList();
        }
    }
}