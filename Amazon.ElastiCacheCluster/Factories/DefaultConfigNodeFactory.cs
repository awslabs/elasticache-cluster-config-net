using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Amazon.ElastiCacheCluster.Factories
{
    internal class DefaultConfigNodeFactory : IConfigNodeFactory
    {
        public IMemcachedNode CreateNode(IPEndPoint endpoint, ISocketPoolConfiguration config)
        {
            return new MemcachedNode(endpoint, config);
        }
    }
}
