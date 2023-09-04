using Biwen.Settings.Domains;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Biwen.Settings.TestWebUI.Settings
{
    public class JsonSettingManager : BaseSettingManager
    {

        private readonly IOptions<SettingOptions> _options;

        private const string jsonPath = "systemsettings.json";
        private readonly static object _lock = new object();

        public JsonSettingManager(ILogger<JsonSettingManager> logger,
            IOptions<SettingOptions> options) : base(logger)
        {
            _options = options;

            if (!File.Exists(jsonPath))
            {
                File.WriteAllText(jsonPath, "[]");
            }
        }

        public override T Get<T>()
        {

            var @default = new T();
            var settingType = typeof(T).FullName!;


            var json = File.ReadAllText(jsonPath);
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
            var json = File.ReadAllText(jsonPath);
            var stored = JsonSerializer.Deserialize<List<Setting>>(json);
            return stored ?? new List<Setting>();
        }

        public override Setting? GetSetting(string settingType)
        {
            var json = File.ReadAllText(jsonPath);
            var stored = JsonSerializer.Deserialize<List<Setting>>(json);
            return stored?.FirstOrDefault(x =>
            x.SettingType == settingType &&
            x.ProjectId == _options.Value.ProjectId);
        }

        public override void Save<T>(T setting)
        {
            lock (_lock)
            {
                var json = File.ReadAllText(jsonPath);
                var stored = JsonSerializer.Deserialize<List<Setting>>(json);

                var @default = new T();
                var desc = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();

                if (stored == null)
                {
                    stored = new();

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

                File.WriteAllText(jsonPath, JsonSerializer.Serialize(stored));
            }
        }
    }
}