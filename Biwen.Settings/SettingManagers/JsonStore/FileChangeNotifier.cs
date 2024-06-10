namespace Biwen.Settings.SettingManagers.JsonStore
{
    internal class FileChangeNotifier(ILogger<FileChangeNotifier> logger, string jsonPath, Action onChange) : IAsyncDisposable
    {
        private readonly string _jsonPath = jsonPath;
        private readonly Action _onChange = onChange;

        FileSystemWatcher watcher = null!;

        /// <summary>
        /// 启动配置文件监听
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Start()
        {
            if (string.IsNullOrEmpty(_jsonPath))
            {
                throw new Exception("JsonPath is empty");
            }

            watcher = new FileSystemWatcher(Path.GetDirectoryName(_jsonPath)!, Path.GetFileName(_jsonPath))
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            watcher.Changed += (sender, e) =>
            {
                logger.LogInformation($"Json文件变更,缓存将清空重新加载!");
                _onChange();
            };
        }

        public async ValueTask DisposeAsync()
        {
            watcher?.Dispose();
            await Task.CompletedTask;
        }
    }
}