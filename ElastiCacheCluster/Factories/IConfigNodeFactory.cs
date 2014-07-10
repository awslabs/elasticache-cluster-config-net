using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ElastiCacheCluster.Factories
{
    public interface IConfigNodeFactory
    {
        IMemcachedNode CreateNode(IPEndPoint endpoint, ISocketPoolConfiguration config);
    }
}
