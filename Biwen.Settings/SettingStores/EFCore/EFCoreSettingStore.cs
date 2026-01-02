// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:30:06 EntityFrameworkCoreSettingStore.cs

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Biwen.Settings.Encryption;

namespace Biwen.Settings.SettingStores.EFCore;


/// <summary>
/// 默认EntityFrameworkCore持久化
/// </summary>
internal sealed class EFCoreSettingStore<TDbContext> : BaseSettingStore where TDbContext : DbContext, IBiwenSettingsDbContext
{
    private readonly IBiwenSettingsDbContext _db;
    private readonly IOptions<SettingOptions> _options;
    private readonly IOptions<EFCoreStoreOptions> _storeOptions;
    private readonly IEncryptionProvider _encryptionProvider;

    public EFCoreSettingStore(
        IServiceProvider serviceProvider,
        //IBiwenSettingsDbContext db,
        ILogger<EFCoreSettingStore<TDbContext>> logger,
        IOptions<EFCoreStoreOptions> storeOptions,
        IEncryptionProvider encryptionProvider,
        IOptions<SettingOptions> options) : base(logger)
    {
        //_db = db;
        _options = options;
        _storeOptions = storeOptions;
        _encryptionProvider = encryptionProvider;

        _db = serviceProvider.GetRequiredService<TDbContext>();
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

    public override async Task<T> GetAsync<T>()
    {
        var @default = new T();
        var settingType = typeof(T).FullName!;

        var setting = await _db.Settings.FirstOrDefaultAsync(
            x => x.ProjectId == _options.Value.ProjectId && x.SettingType == settingType);

        if (setting != null)
        {
            var plainContent = _encryptionProvider.Decrypt(setting.SettingContent!);
            @default = JsonSerializer.Deserialize<T>(plainContent)!;
        }
        else
        {
            var desc = typeof(T).GetCustomAttribute<DescriptionAttribute>(false);
            var plainContent = JsonSerializer.Serialize(@default, SerializerOptions);

            _db.Settings.Add(new Setting
            {
                ProjectId = _options.Value.ProjectId,
                SettingName = @default.SettingName!,
                SettingType = typeof(T).FullName!,
                Description = desc?.Description,
                Order = @default.Order,
                LastModificationTime = DateTime.Now,
                SettingContent = _storeOptions.Value.EncryptionOptions.Enable ? _encryptionProvider.Encrypt(plainContent) : plainContent
            });
            await (_db as DbContext)!.SaveChangesAsync();
        }

        if (@default == null)
        {
            _logger.LogError("SettingType: {FullName} Not Found!", typeof(T).FullName);
            throw new Exception($"SettingType: {typeof(T).FullName} Not Found!");
        }
        //不可为空
        return @default;
    }

    public override async Task SaveAsync<T>(T setting)
    {
        ArgumentNullException.ThrowIfNull(setting, nameof(setting));

        var settingType = typeof(T).FullName!;
        var desc = typeof(T).GetCustomAttribute<DescriptionAttribute>(false);

        var settingContent = JsonSerializer.Serialize(setting, SerializerOptions);
        var settingEntity = _db.Settings.FirstOrDefault(x => x.ProjectId == _options.Value.ProjectId && x.SettingType == settingType);
        if (settingEntity != null)
        {
            settingEntity.SettingContent =
                _storeOptions.Value.EncryptionOptions.Enable ? _encryptionProvider.Encrypt(settingContent) : settingContent;
            settingEntity.LastModificationTime = DateTime.Now;

            settingEntity.Description = desc?.Description;
        }
        else
        {
            var @default = new T();
            _db.Settings.Add(new Setting
            {
                ProjectId = _options.Value.ProjectId,
                SettingName = @default.SettingName!,
                SettingType = typeof(T).FullName!,
                Description = desc?.Description,
                Order = setting.Order,
                LastModificationTime = DateTime.Now,
                SettingContent = _storeOptions.Value.EncryptionOptions.Enable ? _encryptionProvider.Encrypt(settingContent) : settingContent
            });
        }
        await (_db as DbContext)!.SaveChangesAsync();
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