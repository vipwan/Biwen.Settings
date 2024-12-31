// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:30:31 JsonStoreSettingStore.cs

using Biwen.Settings.Encryption;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Biwen.Settings.SettingStores.JsonFile;

public class JsonFileSettingStore : BaseSettingStore
{
    private readonly IOptions<SettingOptions> _options;
    private readonly IOptions<JsonFileStoreOptions> _storeOptions;
    private readonly IEncryptionProvider _encryptionProvider;

    //格式化配置
    private readonly JsonSerializerOptions _serializerOptions;

    private readonly JsonSerializerOptions _contentJsonSerializerOptions = new()
    {
        IgnoreReadOnlyProperties = true,
        IgnoreReadOnlyFields = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        //不缩进
        WriteIndented = false,
        //允许不转义特殊字符
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly static Lock _lock = new();

    public JsonFileSettingStore(ILogger<JsonFileSettingStore> logger,
        IOptions<SettingOptions> options,
        IOptions<JsonFileStoreOptions> storeOptions,
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
            WriteIndented = _storeOptions.Value.FormatJson,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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
            stored =
            [
                .. stored.Where(x => x.ProjectId == _options.Value.ProjectId)
                                    .OrderBy(x => x.Order)
                                    .ThenByDescending(x => x.SettingName),
            ];
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
            var desc = typeof(T).GetCustomAttribute<DescriptionAttribute>(false);

            if (stored == null)
            {
                stored = [];
                var plainContent = JsonSerializer.Serialize(setting, _contentJsonSerializerOptions);

                stored!.Add(new Setting
                {
                    ProjectId = _options.Value.ProjectId,
                    SettingName = setting.SettingName!,
                    SettingType = typeof(T).FullName!,
                    Description = desc?.Description,
                    Order = setting.Order,
                    LastModificationTime = DateTime.Now,
                    SettingContent = _storeOptions.Value.EncryptionOptions.Enable ? _encryptionProvider.Encrypt(plainContent) : plainContent
                });
            }
            else
            {
                stored.RemoveAll(x => x.ProjectId == _options.Value.ProjectId && x.SettingType == typeof(T).FullName);

                var plainContent = JsonSerializer.Serialize(setting, _contentJsonSerializerOptions);

                stored.Add(new Setting
                {
                    ProjectId = _options.Value.ProjectId,
                    SettingName = setting.SettingName!,
                    SettingType = typeof(T).FullName!,
                    Description = desc?.Description,
                    Order = setting.Order,
                    LastModificationTime = DateTime.Now,
                    SettingContent = _storeOptions.Value.EncryptionOptions.Enable ? _encryptionProvider.Encrypt(plainContent) : plainContent
                });
            }
            //Store
            File.WriteAllText(_storeOptions.Value.JsonPath, JsonSerializer.Serialize(stored, _serializerOptions));
        }
    }
}