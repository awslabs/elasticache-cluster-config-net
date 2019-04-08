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
using System.Collections.Generic;
using System.Linq;
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
    internal class AutoServerPool : IServerPool
    {
        private readonly ILogger _log;

        private IMemcachedNode[] _allNodes;

        private readonly IMemcachedClientConfiguration _configuration;
        private readonly IOperationFactory _factory;
        internal IMemcachedNodeLocator NodeLocator;

        private readonly object _deadSync = new object();
        private Timer _resurrectTimer;
        private bool _isTimerActive;
        private readonly long _deadTimeoutMsec;
        private bool _isDisposed;
        private event Action<IMemcachedNode> NodeFailed;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Creates a server pool for auto discovery
        /// </summary>
        /// <param name="configuration">The client configuration using the pool</param>
        /// <param name="opFactory">The factory used to create operations on demand</param>
        /// <param name="loggerFactory">The factory to provide ILogger instance to each class.</param>
        public AutoServerPool(IMemcachedClientConfiguration configuration, IOperationFactory opFactory, ILoggerFactory loggerFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _factory = opFactory ?? throw new ArgumentNullException(nameof(opFactory));

            _deadTimeoutMsec = (long)_configuration.SocketPool.DeadTimeout.TotalMilliseconds;
            _loggerFactory = loggerFactory;
            _log = loggerFactory.CreateLogger<AutoServerPool>();
        }

        ~AutoServerPool()
        {
            try { ((IDisposable)this).Dispose(); }
            catch { /* ignored */ }
        }

        protected virtual IMemcachedNode CreateNode(DnsEndPoint endpoint)
        {
            return new MemcachedNode(endpoint, _configuration.SocketPool,
                _loggerFactory.CreateLogger<MemcachedNode>());
        }

        private void RezCallback(object state)
        {
            _log.LogDebug("Checking the dead servers.");

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
            lock (_deadSync)
            {
                if (_isDisposed)
                {
                    _log.LogWarning("IsAlive timer was triggered but the pool is already disposed. Ignoring.");

                    return;
                }

                var nodes = _allNodes;
                var aliveList = new List<IMemcachedNode>(nodes.Length);
                var changed = false;
                var deadCount = 0;

                for (var i = 0; i < nodes.Length; i++)
                {
                    var n = nodes[i];
                    if (n.IsAlive)
                    {
                        _log.LogDebug("Alive: {EndPoint}", n.EndPoint);

                        aliveList.Add(n);
                    }
                    else
                    {
                        _log.LogDebug("Dead: {EndPoint}", n.EndPoint);

                        if (n.Ping())
                        {
                            changed = true;
                            aliveList.Add(n);

                            _log.LogDebug("Ping ok.");
                        }
                        else
                        {
                            _log.LogDebug("Still dead.");

                            deadCount++;
                        }
                    }
                }

                // reinit the locator
                if (changed)
                {
                    _log.LogDebug("Reinitializing the locator.");

                    NodeLocator.Initialize(aliveList);
                }

                // stop or restart the timer
                if (deadCount == 0)
                {
                    _log.LogDebug("deadCount == 0, stopping the timer.");

                    _isTimerActive = false;
                }
                else
                {
                    _log.LogDebug("deadCount == {deadCount}, starting the timer.", deadCount);

                    _resurrectTimer.Change(_deadTimeoutMsec, Timeout.Infinite);
                }
            }
        }

        private void NodeFail(IMemcachedNode node)
        {
            _log.LogDebug("Node {EndPoint} is dead.", node.EndPoint);

            // the timer is stopped until we encounter the first dead server
            // when we have one, we trigger it and it will run after DeadTimeout has elapsed
            lock (_deadSync)
            {
                if (_isDisposed)
                {
                    _log.LogWarning("Got a node fail but the pool is already disposed. Ignoring.");

                    return;
                }

                // bubble up the fail event to the client
                var fail = NodeFailed;
                if (fail != null)
                    fail(node);

                // re-initialize the locator
                var newLocator = _configuration.CreateNodeLocator();
                newLocator.Initialize(_allNodes.Where(n => n.IsAlive).ToArray());
                Interlocked.Exchange(ref NodeLocator, newLocator);

                // the timer is stopped until we encounter the first dead server
                // when we have one, we trigger it and it will run after DeadTimeout has elapsed
                if (!_isTimerActive)
                {
                    _log.LogDebug("Starting the recovery timer.");

                    if (_resurrectTimer == null)
                        _resurrectTimer = new Timer(RezCallback, null, _deadTimeoutMsec, Timeout.Infinite);
                    else
                        _resurrectTimer.Change(_deadTimeoutMsec, Timeout.Infinite);

                    _isTimerActive = true;

                    _log.LogDebug("Timer started.");
                }
            }
        }

        #region [ IServerPool                  ]

        IMemcachedNode IServerPool.Locate(string key)
        {
            var node = NodeLocator.Locate(key);

            return node;
        }

        IOperationFactory IServerPool.OperationFactory
        {
            get { return _factory; }
        }

        IEnumerable<IMemcachedNode> IServerPool.GetWorkingNodes()
        {
            return NodeLocator.GetWorkingNodes();
        }

        void IServerPool.Start()
        {
            _allNodes = _configuration.Servers.
                                Select(ip =>
                                {
                                    var node = CreateNode(ip);
                                    node.Failed += NodeFail;

                                    return node;
                                }).
                                ToArray();

            // initialize the locator
            var locator = _configuration.CreateNodeLocator();
            locator.Initialize(_allNodes);

            NodeLocator = locator;

            var config = (ElastiCacheClusterConfig)_configuration;
            if (config.Setup.ClusterPoller.IntervalDelay < 0)
                config.DiscoveryNode.StartPoller();
            else
                config.DiscoveryNode.StartPoller(config.Setup.ClusterPoller.IntervalDelay);
        }

        event Action<IMemcachedNode> IServerPool.NodeFailed
        {
            add { NodeFailed += value; }
            remove { NodeFailed -= value; }
        }

        #endregion
        #region [ IDisposable                  ]

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);

            lock (_deadSync)
            {
                if (_isDisposed) return;

                _isDisposed = true;

                // dispose the locator first, maybe it wants to access 
                // the nodes one last time
                var nd = NodeLocator as IDisposable;
                if (nd != null)
                    try { nd.Dispose(); }
                    catch (Exception e) { _log.LogError(e, "Error on disposing a node locator"); }

                NodeLocator = null;

                for (var i = 0; i < _allNodes.Length; i++)
                    try { _allNodes[i].Dispose(); }
                    catch (Exception e) { _log.LogError(e, "Error on disposing the nodes in pool."); }

                // stop the timer
                if (_resurrectTimer != null)
                    using (_resurrectTimer)
                        _resurrectTimer.Change(Timeout.Infinite, Timeout.Infinite);

                _allNodes = null;
                _resurrectTimer = null;
            }
        }

        #endregion

        /// <summary>
        /// Used to update the servers for Auto discovery
        /// </summary>
        /// <param name="endPoints">The connections to all the cluster nodes</param>
        public void UpdateLocator(List<DnsEndPoint> endPoints)
        {
            var newLocator = _configuration.CreateNodeLocator();

            var nodes = endPoints.Select(ip =>
            {
                var node = CreateNode(ip);
                node.Failed += NodeFail;

                return node;
            }).ToArray();

            var aliveList = new List<IMemcachedNode>(nodes.Length);
            var deadList = new List<IMemcachedNode>(nodes.Length);
            foreach (var node in nodes)
            {
                var result = _allNodes.Where(n => n.EndPoint.Equals(node.EndPoint)).ToList();

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

            _allNodes = allNodesList.ToArray();

            Interlocked.Exchange(ref NodeLocator, newLocator);
        }
    }
}
