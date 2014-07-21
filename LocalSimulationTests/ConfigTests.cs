using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amazon.ElastiCacheCluster;
using Enyim.Caching;

namespace LocalSimulationTests
{
    [TestClass]
    public class ConfigTests
    {
        ElastiCacheClusterConfig config;
        MemcachedClient client;

        [TestMethod]
        public void ConfigTest()
        {
            // The url below is used to bypass the .cfg. contraint of the hostname for testing locally
            ClusterConfigSettings settings = new ClusterConfigSettings("www.cfg.org", 11211);
            settings.NodeFactory = new NodeFactory();
            config = new ElastiCacheClusterConfig(settings);
            this.config.DiscoveryNode.Dispose();
        }

        [TestMethod]
        public void InitialRequestTest()
        {
            // The url below is used to bypass the .cfg. contraint of the hostname for testing locally
            ClusterConfigSettings settings = new ClusterConfigSettings("www.cfg.org", 11211);
            settings.NodeFactory = new NodeFactory();
            config = new ElastiCacheClusterConfig(settings);

            client = new MemcachedClient(config);

            Assert.AreEqual(new Version("1.4.14"), config.DiscoveryNode.NodeVersion);
            Assert.AreEqual(1, config.DiscoveryNode.ClusterVersion);
            Assert.AreEqual(3, config.DiscoveryNode.NodesInCluster);

            this.config.DiscoveryNode.Dispose();
            client.Dispose();
        }

        [TestMethod]
        public void PollerTesting()
        {
            //Poller is set to poll every second to make this test faster
            ClusterConfigSettings settings = new ClusterConfigSettings("www.cfg.org", 11211);
            settings.NodeFactory = new NodeFactory();
            settings.ClusterPoller.IntervalDelay = 1000;
            config = new ElastiCacheClusterConfig(settings);

            client = new MemcachedClient(config);

            // Buffer time to wait, this can fail occasionally because delays can occur in the poller or timer
            System.Threading.Thread.Sleep(3000);
            Assert.AreEqual(3, config.DiscoveryNode.ClusterVersion);
            Assert.AreEqual(1, config.DiscoveryNode.NodesInCluster);

            this.config.DiscoveryNode.Dispose();
            client.Dispose();
        }
    }
}
