// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:30:18 FileChangeNotifier.cs

using Biwen.Settings.Caching;
using Microsoft.AspNetCore.Hosting;

namespace Biwen.Settings.SettingStores.JsonFile;

internal class FileChangeNotifier : IAsyncDisposable
{
    private readonly string _jsonPath;
    private readonly Action _onChange;
    private readonly FileSystemWatcher _watcher = null!;

    public FileChangeNotifier(IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var env = sp.GetRequiredService<IWebHostEnvironment>();
        var jsonStoreOptions = sp.GetRequiredService<IOptions<JsonStoreOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<FileChangeNotifier>>();
        var cacheProvider = sp.GetRequiredService<ICacheProvider>();
        var fullFilePath = Path.Combine(env.ContentRootPath, jsonStoreOptions.JsonPath);

        if (string.IsNullOrEmpty(fullFilePath))
        {
            throw new Exception("JsonPath is empty");
        }

        logger.LogInformation($"FileChangeNotifier Started!");

        _jsonPath = fullFilePath;
        _onChange = async () =>
        {
            await cacheProvider.RemoveAllAsync();
        };

        _watcher = new FileSystemWatcher(Path.GetDirectoryName(_jsonPath)!, Path.GetFileName(_jsonPath))
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        _watcher.Changed += (sender, e) =>
        {
            logger.LogInformation($"Json文件变更,缓存将清空重新加载!");
            try
            {
                _onChange();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Json文件变更,缓存重新加载失败!");
            }
        };
    }

    public async ValueTask DisposeAsync()
    {
        _watcher?.Dispose();
        await Task.CompletedTask;
    }
}