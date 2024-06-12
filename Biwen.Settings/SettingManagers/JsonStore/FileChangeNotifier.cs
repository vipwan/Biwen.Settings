using Biwen.Settings.Caching;
using Microsoft.AspNetCore.Hosting;

namespace Biwen.Settings.SettingManagers.JsonStore
{
    internal class FileChangeNotifier : IAsyncDisposable
    {
        private readonly string _jsonPath;
        private readonly Action _onChange;
        private readonly FileSystemWatcher _watcher = null!;

        public FileChangeNotifier(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
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
}