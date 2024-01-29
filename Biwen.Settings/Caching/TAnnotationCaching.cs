using System.Collections.Concurrent;

namespace Biwen.Settings.Caching
{
    internal static class TAnnotationCaching
    {
        /// <summary>
        /// 类型是否标注验证特性
        /// </summary>
        public static readonly ConcurrentDictionary<Type, bool> CachedAnnotationTypes = new();

    }
}