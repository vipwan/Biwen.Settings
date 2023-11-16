using Biwen.Settings.Encryption;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Biwen.Settings.SettingManagers.JsonStore
{
    public class JsonStoreSettingManager : BaseSettingManager
    {
        private readonly IOptions<SettingOptions> _options;
        private readonly IOptions<JsonStoreOptions> _storeOptions;
        private IEncryptionProvider _encryptionProvider;

        //格式化配置
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly static object _lock = new();

        public JsonStoreSettingManager(ILogger<JsonStoreSettingManager> logger,
            IOptions<SettingOptions> options,
            IOptions<JsonStoreOptions> storeOptions,
            IEncryptionProvider encryptionProvider
            )
            : base(logger)
        {
            _options = options;
            _storeOptions = storeOptions;
            _encryptionProvider = encryptionProvider;
            _serializerOptions = new()
            {
                IgnoreReadOnlyProperties = true,
                IgnoreReadOnlyFields = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = _storeOptions.Value.FormatJson
            };

            if (!File.Exists(_storeOptions.Value.JsonPath))
            {
                File.WriteAllText(_storeOptions.Value.JsonPath, "[]");
            }
        }

        public override T Get<T>()
        {

            var @default = new T();
            var settingType = typeof(T).FullName!;


            var json = File.ReadAllText(_storeOptions.Value.JsonPath);
            var stored = JsonSerializer.Deserialize<List<Setting>>(json)?.FirstOrDefault(
                x => x.ProjectId == _options.Value.ProjectId && x.SettingType == settingType);

            if (stored != null)
            {
                var plainContent = _encryptionProvider.Decrypt(stored.SettingContent!);
                return JsonSerializer.Deserialize<T>(plainContent)!;
            }

            Save(@default);

            return @default;
        }

        public override List<Setting> GetAllSettings()
        {
            var json = File.ReadAllText(_storeOptions.Value.JsonPath);
            var stored = JsonSerializer.Deserialize<List<Setting>>(json);

            if (stored != null)
            {
                stored = stored.Where(x => x.ProjectId == _options.Value.ProjectId)
                    .OrderBy(x => x.Order)
                    .ThenByDescending(x => x.SettingName).ToList();
                return stored;
            }
            return [];
        }

        public override Setting? GetSetting(string settingType)
        {
            var json = File.ReadAllText(_storeOptions.Value.JsonPath);
            var stored = JsonSerializer.Deserialize<List<Setting>>(json);
            return stored?.FirstOrDefault(x =>
            x.SettingType == settingType &&
            x.ProjectId == _options.Value.ProjectId);
        }



        public override void Save<T>(T setting)
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_storeOptions.Value.JsonPath);
                var stored = JsonSerializer.Deserialize<List<Setting>>(json);

                var @default = new T();
                var desc = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();

                if (stored == null)
                {
                    stored = [];
                    var plainContent = JsonSerializer.Serialize(setting);

                    stored!.Add(new Setting
                    {
                        ProjectId = _options.Value.ProjectId,
                        SettingName = setting.SettingName!,
                        SettingType = typeof(T).FullName!,
                        Description = desc != null ? ((DescriptionAttribute)desc).Description : null,
                        Order = setting.Order,
                        LastModificationTime = DateTime.Now,
                        SettingContent = _storeOptions.Value.EncryptionOption.Enable ? _encryptionProvider.Encrypt(plainContent) : plainContent
                    });
                }
                else
                {
                    stored.RemoveAll(x => x.ProjectId == _options.Value.ProjectId && x.SettingType == typeof(T).FullName);

                    var plainContent = JsonSerializer.Serialize(setting);

                    stored.Add(new Setting
                    {
                        ProjectId = _options.Value.ProjectId,
                        SettingName = setting.SettingName!,
                        SettingType = typeof(T).FullName!,
                        Description = desc != null ? ((DescriptionAttribute)desc).Description : null,
                        Order = setting.Order,
                        LastModificationTime = DateTime.Now,
                        SettingContent = _storeOptions.Value.EncryptionOption.Enable ? _encryptionProvider.Encrypt(plainContent) : plainContent
                    });
                }
                //Store
                File.WriteAllText(_storeOptions.Value.JsonPath, JsonSerializer.Serialize(stored, _serializerOptions));
            }
        }
    }
}