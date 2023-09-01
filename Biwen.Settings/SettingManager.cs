
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Biwen.Settings.Caching;

namespace Biwen.Settings
{
    internal sealed class SettingManager : ISettingManager
    {
        private readonly IBiwenSettingsDbContext _db;
        private readonly ILogger<SettingManager> _logger;
        private readonly IOptions<SettingOptions> _options;
        private readonly ICacheProvider _cacheProvider;

        private const string CacheKeyFormat = "SettingManager_{0}";

        public SettingManager(
            IBiwenSettingsDbContext db,
            ILogger<SettingManager> logger,
            ICacheProvider cacheProvider,
            IOptions<SettingOptions> options)
        {
            _db = db;
            _logger = logger;
            _cacheProvider = cacheProvider;
            _options = options;
        }

        /// <summary>
        /// 序列化配置项时的选项
        /// </summary>
        readonly JsonSerializerOptions SerializerOptions = new()
        {
            IgnoreReadOnlyProperties = true,
            IgnoreReadOnlyFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public T Get<T>() where T : ISetting, new()
        {
            return (T)_cacheProvider.GetOrCreate(string.Format(CacheKeyFormat, typeof(T).FullName), () =>
              {
                  var @default = new T();
                  var settingType = typeof(T).FullName!;

                  var setting = _db.Settings.FirstOrDefault(
                      x => x.ProjectId == _options.Value.ProjectId && x.SettingType == settingType);

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
                          SettingType = typeof(T).FullName!,
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

              }, 1000);
        }

        public void Save<T>(T setting) where T : ISetting, new()
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            var settingType = typeof(T).FullName!;

            var settingContent = JsonSerializer.Serialize(setting, SerializerOptions);
            var settingEntity = _db.Settings.FirstOrDefault(x => x.ProjectId == _options.Value.ProjectId && x.SettingType == settingType);
            if (settingEntity != null)
            {
                settingEntity.SettingContent = settingContent;
                settingEntity.LastModificationTime = DateTime.Now;
            }
            else
            {

                var @default = new T();

                var desc = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                _db.Settings.Add(new Setting
                {
                    ProjectId = _options.Value.ProjectId,
                    SettingName = @default.SettingName!,
                    SettingType = typeof(T).FullName!,
                    Description = desc != null ? ((DescriptionAttribute)desc).Description : null,
                    Order = setting.Order,
                    LastModificationTime = DateTime.Now,
                    SettingContent = settingContent
                });
            }
            (_db as DbContext)!.SaveChanges();
            _cacheProvider.Remove(string.Format(CacheKeyFormat, typeof(T).Name));

            _logger.LogInformation(message: "SaveSetting: {0},{1}", settingType, settingContent);
        }

        public List<Setting> GetAllSettings()
        {
            return _db.Settings.Where(x => x.ProjectId == _options.Value.ProjectId).OrderBy(x => x.Order).ThenBy(x => x.SettingType).ToList();
        }
    }
}