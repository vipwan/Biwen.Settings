using System.Collections.Concurrent;

namespace Biwen.Settings.Caching
{
    internal static class TAnnotationCaching
    {
        public static readonly ConcurrentDictionary<Type, bool> TAnnotationAttrs = new();

    }
}