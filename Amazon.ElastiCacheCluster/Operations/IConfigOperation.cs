using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching.Memcached;

namespace Amazon.ElastiCacheCluster.Operations
{
    public interface IConfigOperation
    {
        CacheItem ConfigResult { get; set; }
    }
}
