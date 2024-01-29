using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Biwen.Settings.Encryption;

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
        private readonly IEncryptionProvider _encryptionProvider;

        public EntityFrameworkCoreSettingManager(
            IServiceProvider serviceProvider,
            //IBiwenSettingsDbContext db,
            ILogger<EntityFrameworkCoreSettingManager> logger,
            IOptions<EFCoreStoreOptions> storeOptions,
            IEncryptionProvider encryptionProvider,
            IOptions<SettingOptions> options) : base(logger)
        {
            //_db = db;
            _options = options;
            _storeOptions = storeOptions;
            _encryptionProvider = encryptionProvider;

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
                var plainContent = _encryptionProvider.Decrypt(setting.SettingContent!);
                @default = JsonSerializer.Deserialize<T>(plainContent)!;
            }
            else
            {
                var desc = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();

                var plainContent = JsonSerializer.Serialize(@default, SerializerOptions);

                _db.Settings.Add(new Setting
                {
                    ProjectId = _options.Value.ProjectId,
                    SettingName = @default.SettingName!,
                    SettingType = typeof(T).FullName!,
                    Description = desc != null ? ((DescriptionAttribute)desc).Description : null,
                    Order = @default.Order,
                    LastModificationTime = DateTime.Now,
                    SettingContent = _storeOptions.Value.EncryptionOption.Enable ? _encryptionProvider.Encrypt(plainContent) : plainContent
                });
                (_db as DbContext)!.SaveChanges();
            }

            if (@default == null)
            {
                _logger.LogError("SettingType: {FullName} Not Found!", typeof(T).FullName);
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
                settingEntity.SettingContent =
                    _storeOptions.Value.EncryptionOption.Enable ? _encryptionProvider.Encrypt(settingContent) : settingContent;
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
                    SettingContent = _storeOptions.Value.EncryptionOption.Enable ? _encryptionProvider.Encrypt(settingContent) : settingContent
                });
            }
            (_db as DbContext)!.SaveChanges();
            _logger.LogInformation("SaveSetting: {settingType},{settingContent}", settingType, settingContent);
        }

        public override List<Setting> GetAllSettings()
        {
            return [.. _db.Settings.Where(x => x.ProjectId == _options.Value.ProjectId).OrderBy(x => x.Order).ThenBy(x => x.SettingType)];
        }


        public override Setting? GetSetting(string settingType)
        {
            return _db.Settings.FirstOrDefault(x => x.ProjectId == _options.Value.ProjectId && x.SettingType == settingType);
        }
    }
}