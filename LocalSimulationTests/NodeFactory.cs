using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElastiCacheCluster.Factories;
using Enyim.Caching.Memcached;

namespace LocalSimulationTests
{
    public class NodeFactory : IConfigNodeFactory
    {
        TestNode node;

        public NodeFactory()
        {
            this.node = new TestNode();
        }

        public IMemcachedNode CreateNode(System.Net.IPEndPoint endpoint, Enyim.Caching.Configuration.ISocketPoolConfiguration config)
        {
            return node;
        }
    }
}
