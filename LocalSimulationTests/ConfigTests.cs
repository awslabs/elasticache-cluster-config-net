/*
 * Copyright 2014 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *  
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://aws.amazon.com/apache2.0
 * 
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amazon.ElastiCacheCluster;
using Enyim.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace LocalSimulationTests
{
    [TestClass]
    public class ConfigTests 
    {
        ElastiCacheClusterConfig config;
        MemcachedClient client;
        private ILoggerFactory loggerFactory;

        [TestInitialize]
        public void Setup()
        {
            var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>("", null);
            var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new []{ configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
            var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());
            loggerFactory = new LoggerFactory(new[] { new ConsoleLoggerProvider(optionsMonitor) }, new LoggerFilterOptions { MinLevel = LogLevel.Trace });
        }
        
        [TestCleanup]
        public void Teardown()
        {
            loggerFactory.Dispose();
        }

        [TestMethod]
        public void ConfigTest()
        {
            // The url below is used to bypass the .cfg. contraint of the hostname for testing locally
            ClusterConfigSettings settings = new ClusterConfigSettings("www.cfg.org", 11211);
            settings.NodeFactory = new NodeFactory();
            config = new ElastiCacheClusterConfig(loggerFactory, settings);
            this.config.DiscoveryNode.Dispose();
        }

        [TestMethod]
        public void InitialRequestTest()
        {
            // The url below is used to bypass the .cfg. contraint of the hostname for testing locally
            ClusterConfigSettings settings = new ClusterConfigSettings("www.cfg.org", 11211);
            settings.NodeFactory = new NodeFactory();
            config = new ElastiCacheClusterConfig(loggerFactory, settings);

            client = new MemcachedClient(loggerFactory, config);

            Assert.AreEqual(new Version("1.4.14"), config.DiscoveryNode.NodeVersion);
            Assert.AreEqual(1, config.DiscoveryNode.ClusterVersion);
            Assert.AreEqual(3, config.DiscoveryNode.NodesInCluster);

            this.config.DiscoveryNode.Dispose();
            client.Dispose();
        }

        [TestMethod]
        public async Task PollerTesting()
        {
            //Poller is set to poll every second to make this test faster
            ClusterConfigSettings settings = new ClusterConfigSettings("www.cfg.org", 11211);
            settings.NodeFactory = new NodeFactory();
            settings.ClusterPoller.IntervalDelay = 1000;
            config = new ElastiCacheClusterConfig(loggerFactory, settings);

            client = new MemcachedClient(loggerFactory, config);

            // Buffer time to wait, this can fail occasionally because delays can occur in the poller or timer
            await Task.Delay(3000);
//            System.Threading.Thread.Sleep(3000);
            Assert.AreEqual(3, config.DiscoveryNode.ClusterVersion);
            Assert.AreEqual(1, config.DiscoveryNode.NodesInCluster);

            this.config.DiscoveryNode.Dispose();
            client.Dispose();
        }
    }
}
