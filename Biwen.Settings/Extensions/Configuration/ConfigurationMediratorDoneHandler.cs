namespace Biwen.Settings.Extensions.Configuration
{
    /// <summary>
    /// 写入Channel消费
    /// </summary>
    internal class ConfigurationMediratorDoneHandler(ILogger<ConfigurationMediratorDoneHandler> logger) : IMediratorDoneHandler
    {
        public Task OnPublishedAsync<T>(T @event) where T : ISetting, new()
        {
            Events.ConfigrationChangedChannel.Writer.TryWrite((true, typeof(T).Name));
            logger.LogInformation($"Setting Changed: {typeof(T).Name},并通知Configuration刷新!");
            return Task.CompletedTask;
        }
    }
}