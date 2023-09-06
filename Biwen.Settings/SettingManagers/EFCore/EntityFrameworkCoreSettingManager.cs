
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.Settings.SettingManagers.EFCore
{

    /// <summary>
    /// 默认EntityFrameworkCore持久化
    /// </summary>
    internal sealed class EntityFrameworkCoreSettingManager : BaseSettingManager
    {
        private readonly IBiwenSettingsDbContext _db;
        private readonly IOptions<SettingOptions> _options;
        private readonly IOptions<EFCoreStoreOptions> _storeOptions;

        public EntityFrameworkCoreSettingManager(
            IServiceProvider serviceProvider,
            //IBiwenSettingsDbContext db,
            ILogger<EntityFrameworkCoreSettingManager> logger,
            IOptions<EFCoreStoreOptions> storeOptions,
            IOptions<SettingOptions> options) : base(logger)
        {
            //_db = db;
            _options = options;
            _storeOptions = storeOptions;

            if (_storeOptions!.Value.DbContextType == null ||
                !_storeOptions!.Value.DbContextType.IsAssignableTo(typeof(IBiwenSettingsDbContext)))
            {
                throw new BiwenException("DbContextType not null & must be inherited from IBiwenSettingsDbContext");
            }
            _db = (serviceProvider.GetRequiredService(_storeOptions.Value.DbContextType) as IBiwenSettingsDbContext)!;
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

        public override T Get<T>()
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
        }

        public override void Save<T>(T setting)
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
            _logger.LogInformation(message: "SaveSetting: {0},{1}", settingType, settingContent);
        }

        public override List<Setting> GetAllSettings()
        {
            return _db.Settings.Where(x => x.ProjectId == _options.Value.ProjectId).OrderBy(x => x.Order).ThenBy(x => x.SettingType).ToList();
        }


        public override Setting? GetSetting(string settingType)
        {
            return _db.Settings.FirstOrDefault(x => x.ProjectId == _options.Value.ProjectId && x.SettingType == settingType);
        }
    }
}