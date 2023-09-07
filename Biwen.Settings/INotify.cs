using Microsoft.Extensions.DependencyInjection;

namespace Biwen.Settings
{
    /// <summary>
    /// T变更提醒
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INotify<T> where T : ISetting, new()
    {
        /// <summary>
        /// 实现提醒业务
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        Task NotifyAsync(T setting);

        /// <summary>
        /// 是否异步执行(true无需等待)
        /// </summary>
        bool IsAsync { get; }


    }


    public abstract class BaseNotify<T> : INotify<T> where T : ISetting, new()
    {
        public abstract Task NotifyAsync(T setting);
        /// <summary>
        /// 默认异步执行
        /// </summary>
        public virtual bool IsAsync => true;
    }

    /// <summary>
    /// Medirator Of T
    /// </summary>
    internal interface IMedirator
    {
        /// <summary>
        /// Publish T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        Task PublishAsync<T>(T @event) where T : ISetting, new();
    }

    internal class Medirator : IMedirator
    {
        private readonly IServiceProvider _serviceProvider;
        public Medirator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<T>(T @event) where T : ISetting, new()
        {
            var notifys = _serviceProvider.GetServices<INotify<T>>();
            foreach (var notify in notifys)
            {
                if (notify.IsAsync)
                {
                    _ = notify.NotifyAsync(@event);
                }
                else
                {
                    await notify.NotifyAsync(@event);
                }
            }

        }
    }
}