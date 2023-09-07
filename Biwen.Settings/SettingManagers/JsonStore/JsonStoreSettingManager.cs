using Biwen.Settings.SettingManagers.JsonStore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Biwen.Settings.TestWebUI.Settings
{
    public class JsonStoreSettingManager : BaseSettingManager
    {
        private readonly IOptions<SettingOptions> _options;
        private readonly IOptions<JsonStoreOptions> _storeOptions;
        //格式化配置
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly static object _lock = new();

        public JsonStoreSettingManager(ILogger<JsonStoreSettingManager> logger,
            IOptions<SettingOptions> options,
            IOptions<JsonStoreOptions> storeOptions
            )
            : base(logger)
        {
            _options = options;
            _storeOptions = storeOptions;

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
                return JsonSerializer.Deserialize<T>(stored.SettingContent!)!;
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
            return new List<Setting>();
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
                    stored = new();
                    stored!.Add(new Setting
                    {
                        ProjectId = _options.Value.ProjectId,
                        SettingName = setting.SettingName!,
                        SettingType = typeof(T).FullName!,
                        Description = desc != null ? ((DescriptionAttribute)desc).Description : null,
                        Order = setting.Order,
                        LastModificationTime = DateTime.Now,
                        SettingContent = JsonSerializer.Serialize(setting)
                    });
                }
                else
                {
                    stored.RemoveAll(x => x.ProjectId == _options.Value.ProjectId && x.SettingType == typeof(T).FullName);
                    stored.Add(new Setting
                    {
                        ProjectId = _options.Value.ProjectId,
                        SettingName = setting.SettingName!,
                        SettingType = typeof(T).FullName!,
                        Description = desc != null ? ((DescriptionAttribute)desc).Description : null,
                        Order = setting.Order,
                        LastModificationTime = DateTime.Now,
                        SettingContent = JsonSerializer.Serialize(setting)
                    });
                }
                //Store
                File.WriteAllText(_storeOptions.Value.JsonPath, JsonSerializer.Serialize(stored, _serializerOptions));
            }
        }
    }
}