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
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Logging;

namespace Amazon.ElastiCacheCluster
{
    /// <summary>
    /// A poller used to reconfigure the client servers when updates occur to the cluster configuration
    /// </summary>
    internal class ConfigurationPoller
    {
        private readonly ILogger log;

        #region Defaults

        // Poll once every minute
        private static readonly int DEFAULT_INTERVAL_DELAY = 60000;

        #endregion

        private Timer timer;
        private int intervalDelay;
        private ElastiCacheClusterConfig config;

        #region Constructors

        /// <summary>
        /// Creates a poller for Auto Discovery with the default intervals
        /// </summary>
        /// <param name="client">The memcached client to update servers for</param>
        public ConfigurationPoller(ElastiCacheClusterConfig config)
            : this(config, DEFAULT_INTERVAL_DELAY) { }

        /// <summary>
        /// Creates a poller for Auto Discovery with the defined itnerval, delay, tries, and try delay for polling
        /// </summary>
        /// <param name="client">The memcached client to update servers for</param>
        /// <param name="intervalDelay">The amount of time between polling operations in miliseconds</param>
        public ConfigurationPoller(ElastiCacheClusterConfig config, int intervalDelay)
        {
            this.intervalDelay = intervalDelay < 0 ? DEFAULT_INTERVAL_DELAY : intervalDelay;
            this.config = config;
            log = config.LoggerFactory.CreateLogger<ConfigurationPoller>();

            this.timer = new Timer(this.intervalDelay);
            this.timer.Elapsed += this.pollOnTimedEvent;
        }

        #endregion

        #region Polling Methods

        internal void StartTimer()
        {
            log.LogDebug("Starting timer");
            this.pollOnTimedEvent(null, null);
            this.timer.Start();
        }

        /// <summary>
        /// Used by the poller's timer to update the cluster configuration if a new version is available
        /// </summary>
        internal void pollOnTimedEvent(Object source, ElapsedEventArgs evnt)
        {
            log.LogDebug("Polling...");
            try
            {
                var oldVersion = config.DiscoveryNode.ClusterVersion;
                var endPoints = config.DiscoveryNode.GetEndPointList();
                if (oldVersion != config.DiscoveryNode.ClusterVersion || 
                    (this.config.Pool.nodeLocator != null && endPoints.Count != this.config.Pool.nodeLocator.GetWorkingNodes().Count()))
                {
                    log.LogDebug("Updating endpoints to have {Count} nodes", endPoints.Count);
                    this.config.Pool.UpdateLocator(endPoints);
                }
            }
            catch(Exception e)
            {
                try
                {
                    log.LogDebug("Error updating endpoints, going to attempt to reresolve configuration endpoint.", e);
                    config.DiscoveryNode.ResolveEndPoint();

                    var oldVersion = config.DiscoveryNode.ClusterVersion;
                    var endPoints = config.DiscoveryNode.GetEndPointList();
                    if (oldVersion != config.DiscoveryNode.ClusterVersion ||
                        (this.config.Pool.nodeLocator != null && endPoints.Count != this.config.Pool.nodeLocator.GetWorkingNodes().Count()))
                    {
                        log.LogDebug("Updating endpoints to have {Count} nodes", endPoints.Count);
                        this.config.Pool.UpdateLocator(endPoints);
                    }
                }
                catch (Exception ex)
                {
                    log.LogDebug("Error updating endpoints. Setting endpoints to empty collection of nodes.", ex);

                    /* 
                     * We were not able to retrieve the current node configuration. This is most likely because the application
                     * is running in development outside of EC2. ElastiCache clusters are only accessible from an EC2 instance
                     * with the right security permissions.
                     */
                    this.config.Pool.UpdateLocator(new List<System.Net.DnsEndPoint>());
                }
            }
        }

        #endregion

        /// <summary>
        /// Disposes the background thread that is used for polling the configs
        /// </summary>
        public void StopPolling()
        {
            log.LogDebug("Destroying poller thread");
            timer?.Dispose();
        }
    }
}
