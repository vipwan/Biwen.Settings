using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.Settings.Caching
{
    /// <summary>
    /// 默认的缓存提供者,不做任何缓存
    /// </summary>
    internal sealed class NullCacheProvider : ICacheProvider
    {

        public object GetOrCreate(string key, Func<object> factory, int cacheTime = 86400)
        {
            return factory();
        }

        public void Remove(string key)
        {
        }
    }
}