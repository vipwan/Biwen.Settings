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
        Task Notify(T setting);
    }

}