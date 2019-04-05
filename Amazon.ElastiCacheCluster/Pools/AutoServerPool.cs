/*
 * Copyright 2014 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Portions copyright 2010 Attila Kiskó, enyim.com. Please see LICENSE.txt
 * for applicable license terms and NOTICE.txt for applicable notices.
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
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Microsoft.Extensions.Logging;

namespace Amazon.ElastiCacheCluster.Pools
{
    /// <summary>
    /// A server pool just like the default that enables safely changing the servers of the locator
    /// </summary>
    internal class AutoServerPool : IServerPool, IDisposable
    {
        private readonly ILogger log;

        private IMemcachedNode[] allNodes;

        private IMemcachedClientConfiguration configuration;
        private IOperationFactory factory;
        internal IMemcachedNodeLocator nodeLocator;

        private object DeadSync = new Object();
        private System.Threading.Timer resurrectTimer;
        private bool isTimerActive;
        private long deadTimeoutMsec;
        private bool isDisposed;
        private event Action<IMemcachedNode> nodeFailed;
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Creates a server pool for auto discovery
        /// </summary>
        /// <param name="configuration">The client configuration using the pool</param>
        /// <param name="opFactory">The factory used to create operations on demand</param>
        public AutoServerPool(IMemcachedClientConfiguration configuration, IOperationFactory opFactory, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.factory = opFactory ?? throw new ArgumentNullException(nameof(opFactory));

            this.deadTimeoutMsec = (long)this.configuration.SocketPool.DeadTimeout.TotalMilliseconds;
            this.loggerFactory = loggerFactory;
            log = loggerFactory.CreateLogger<AutoServerPool>();
        }

        ~AutoServerPool()
        {
            try { ((IDisposable)this).Dispose(); }
            catch { }
        }

        protected virtual IMemcachedNode CreateNode(DnsEndPoint endpoint)
        {
            return new MemcachedNode(endpoint, this.configuration.SocketPool,
                loggerFactory.CreateLogger<MemcachedNode>());
        }

        private void rezCallback(object state)
        {
            log.LogDebug("Checking the dead servers.");

            // how this works:
            // 1. timer is created but suspended
            // 2. Locate encounters a dead server, so it starts the timer which will trigger after deadTimeout has elapsed
            // 3. if another server goes down before the timer is triggered, nothing happens in Locate (isRunning == true).
            //		however that server will be inspected sooner than Dead Timeout.
            //		   S1 died   S2 died    dead timeout
            //		|----*--------*------------*-
            //           |                     |
            //          timer start           both servers are checked here
            // 4. we iterate all the servers and record it in another list
            // 5. if we found a dead server whihc responds to Ping(), the locator will be reinitialized
            // 6. if at least one server is still down (Ping() == false), we restart the timer
            // 7. if all servers are up, we set isRunning to false, so the timer is suspended
            // 8. GOTO 2
            lock (this.DeadSync)
            {
                if (this.isDisposed)
                {
                    log.LogWarning("IsAlive timer was triggered but the pool is already disposed. Ignoring.");

                    return;
                }

                var nodes = this.allNodes;
                var aliveList = new List<IMemcachedNode>(nodes.Length);
                var changed = false;
                var deadCount = 0;

                for (var i = 0; i < nodes.Length; i++)
                {
                    var n = nodes[i];
                    if (n.IsAlive)
                    {
                        log.LogDebug("Alive: {EndPoint}", n.EndPoint);

                        aliveList.Add(n);
                    }
                    else
                    {
                        log.LogDebug("Dead: {EndPoint}", n.EndPoint);

                        if (n.Ping())
                        {
                            changed = true;
                            aliveList.Add(n);

                            log.LogDebug("Ping ok.");
                        }
                        else
                        {
                            log.LogDebug("Still dead.");

                            deadCount++;
                        }
                    }
                }

                // reinit the locator
                if (changed)
                {
                    log.LogDebug("Reinitializing the locator.");

                    this.nodeLocator.Initialize(aliveList);
                }

                // stop or restart the timer
                if (deadCount == 0)
                {
                    log.LogDebug("deadCount == 0, stopping the timer.");

                    this.isTimerActive = false;
                }
                else
                {
                    log.LogDebug("deadCount == {deadCount}, starting the timer.", deadCount);

                    this.resurrectTimer.Change(this.deadTimeoutMsec, Timeout.Infinite);
                }
            }
        }

        private void NodeFail(IMemcachedNode node)
        {
            log.LogDebug("Node {EndPoint} is dead.", node.EndPoint);

            // the timer is stopped until we encounter the first dead server
            // when we have one, we trigger it and it will run after DeadTimeout has elapsed
            lock (this.DeadSync)
            {
                if (this.isDisposed)
                {
                    log.LogWarning("Got a node fail but the pool is already disposed. Ignoring.");

                    return;
                }

                // bubble up the fail event to the client
                var fail = this.nodeFailed;
                if (fail != null)
                    fail(node);

                // re-initialize the locator
                var newLocator = this.configuration.CreateNodeLocator();
                newLocator.Initialize(allNodes.Where(n => n.IsAlive).ToArray());
                Interlocked.Exchange(ref this.nodeLocator, newLocator);

                // the timer is stopped until we encounter the first dead server
                // when we have one, we trigger it and it will run after DeadTimeout has elapsed
                if (!this.isTimerActive)
                {
                    log.LogDebug("Starting the recovery timer.");

                    if (this.resurrectTimer == null)
                        this.resurrectTimer = new Timer(this.rezCallback, null, this.deadTimeoutMsec, Timeout.Infinite);
                    else
                        this.resurrectTimer.Change(this.deadTimeoutMsec, Timeout.Infinite);

                    this.isTimerActive = true;

                    log.LogDebug("Timer started.");
                }
            }
        }

        #region [ IServerPool                  ]

        IMemcachedNode IServerPool.Locate(string key)
        {
            var node = this.nodeLocator.Locate(key);

            return node;
        }

        IOperationFactory IServerPool.OperationFactory
        {
            get { return this.factory; }
        }

        IEnumerable<IMemcachedNode> IServerPool.GetWorkingNodes()
        {
            return this.nodeLocator.GetWorkingNodes();
        }

        void IServerPool.Start()
        {
            this.allNodes = this.configuration.Servers.
                                Select(ip =>
                                {
                                    var node = this.CreateNode(ip);
                                    node.Failed += this.NodeFail;

                                    return node;
                                }).
                                ToArray();

            // initialize the locator
            var locator = this.configuration.CreateNodeLocator();
            locator.Initialize(allNodes);

            this.nodeLocator = locator;

            var config = this.configuration as ElastiCacheClusterConfig;
            if (config.setup.ClusterPoller.IntervalDelay < 0)
                config.DiscoveryNode.StartPoller();
            else
                config.DiscoveryNode.StartPoller(config.setup.ClusterPoller.IntervalDelay);
        }

        event Action<IMemcachedNode> IServerPool.NodeFailed
        {
            add { this.nodeFailed += value; }
            remove { this.nodeFailed -= value; }
        }

        #endregion
        #region [ IDisposable                  ]

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);

            lock (this.DeadSync)
            {
                if (this.isDisposed) return;

                this.isDisposed = true;

                // dispose the locator first, maybe it wants to access 
                // the nodes one last time
                var nd = this.nodeLocator as IDisposable;
                if (nd != null)
                    try { nd.Dispose(); }
                    catch (Exception e) { log.LogError(e, "Error on disposing a node locator"); }

                this.nodeLocator = null;

                for (var i = 0; i < this.allNodes.Length; i++)
                    try { this.allNodes[i].Dispose(); }
                    catch (Exception e) { log.LogError(e, "Error on disposing the nodes in pool."); }

                // stop the timer
                if (this.resurrectTimer != null)
                    using (this.resurrectTimer)
                        this.resurrectTimer.Change(Timeout.Infinite, Timeout.Infinite);

                this.allNodes = null;
                this.resurrectTimer = null;
            }
        }

        #endregion

        /// <summary>
        /// Used to update the servers for Auto discovery
        /// </summary>
        /// <param name="endPoints">The connections to all the cluster nodes</param>
        public void UpdateLocator(List<DnsEndPoint> endPoints)
        {
            var newLocator = this.configuration.CreateNodeLocator();

            var nodes = endPoints.Select(ip =>
            {
                var node = this.CreateNode(ip);
                node.Failed += this.NodeFail;

                return node;
            }).ToArray();

            var aliveList = new List<IMemcachedNode>(nodes.Length);
            var deadList = new List<IMemcachedNode>(nodes.Length);
            foreach (var node in nodes)
            {
                var result = this.allNodes.Where(n => n.EndPoint.Equals(node.EndPoint)).ToList();

                if (result.Count > 0 && !result[0].IsAlive)
                {
                    deadList.Add(result[0]);
                    continue;
                }

                aliveList.Add(node);
            }

            newLocator.Initialize(aliveList);

            // Retain All Nodes List With IsAlive Status
            var allNodesList = new List<IMemcachedNode>(nodes.Length);
            allNodesList.AddRange(aliveList);
            allNodesList.AddRange(deadList);

            this.allNodes = allNodesList.ToArray();

            Interlocked.Exchange(ref this.nodeLocator, newLocator);
        }
    }
}
