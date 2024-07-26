namespace Biwen.Settings.Extensions.Configuration
{
    /// <summary>
    /// 写入Channel消费
    /// </summary>
    internal class ConfigurationMediratorDoneHandler(ILogger<ConfigurationMediratorDoneHandler> logger) : IMediratorDoneHandler
    {
        public Task OnPublishedAsync<T>(T @event) where T : ISetting, new()
        {
            Events.ConfigrationChangedChannel.Writer.TryWrite((true, typeof(T)));
            logger.LogInformation("Setting Changed: {Name},并通知Configuration刷新!", typeof(T).Name);
            return Task.CompletedTask;
        }
    }
}