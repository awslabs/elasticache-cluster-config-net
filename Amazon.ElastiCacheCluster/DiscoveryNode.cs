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
using System.Net;
using System.Text;
using System.Threading;
using Amazon.ElastiCacheCluster.Helpers;
using Amazon.ElastiCacheCluster.Operations;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Protocol.Text;
using Microsoft.Extensions.Logging;
using GetOperation = Amazon.ElastiCacheCluster.Operations.GetOperation;

namespace Amazon.ElastiCacheCluster
{
    /// <summary>
    /// A class that manages the discovery of endpoints inside of an ElastiCache cluster
    /// </summary>
    public class DiscoveryNode
    {
        #region Static ReadOnlys

        private readonly ILogger _log;

        internal const int DefaultTryCount = 5;
        internal const int DefaultTryDelay = 1000;

        #endregion

        /// <summary>
        /// The version of memcached running on the Nodes
        /// </summary>
        public Version NodeVersion { get; private set; }

        /// <summary>
        /// The version of the cluster configuration
        /// </summary>
        public int ClusterVersion { get; private set; }

        /// <summary>
        /// The number of nodes running inside of the cluster
        /// </summary>
        public int NodesInCluster => _nodes.Count;

        #region Private Fields

        private DnsEndPoint _endPoint;

        private IMemcachedNode _node;

        private readonly ElastiCacheClusterConfig _config;

        private readonly List<IMemcachedNode> _nodes = new List<IMemcachedNode>();

        private ConfigurationPoller _poller;

        private readonly string _hostname;
        private readonly int _port;

        private readonly int _tries;
        private readonly int _delay;

        private readonly object _nodesLock;
        private readonly object _endpointLock;
        private readonly object _clusterLock;

        #endregion

        #region Constructors

        /// <summary>
        /// The node used to discover endpoints in an ElastiCache cluster
        /// </summary>
        /// <param name="config">The config of the client to access the SocketPool</param>
        /// <param name="hostname">The host name of the cluster with .cfg. in name</param>
        /// <param name="port">The port of the cluster</param>
        /// <param name="tries">The number of tries for requesting config info</param>
        /// <param name="delay">The time, in miliseconds, to wait between tries</param>
        internal DiscoveryNode(ElastiCacheClusterConfig config, string hostname, int port, 
            int tries = DefaultTryCount, int delay = DefaultTryDelay)
        {
            #region Param Checks

            if (string.IsNullOrEmpty(hostname))
                throw new ArgumentNullException(nameof(hostname));
            if (port <= 0)
                throw new ArgumentException("Port cannot be 0 or less");
            if (tries < 1)
                throw new ArgumentException("Must atleast try once");
            if (delay < 0)
                throw new ArgumentException("The delay can't be negative");
            if (hostname.IndexOf(".cfg", StringComparison.OrdinalIgnoreCase) < 0)
                throw new ArgumentException("The hostname is not able to use Auto Discovery");

            #endregion

            #region Setting Members

            _hostname = hostname;
            _port = port;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            ClusterVersion = 0;
            _tries = tries;
            _delay = delay;
            _log = config.LoggerFactory.CreateLogger<DiscoveryNode>();

            _clusterLock = new object();
            _endpointLock = new object();
            _nodesLock = new object();

            #endregion

            ResolveEndPoint();
        }

        #endregion

        #region Poller Methods

        /// <summary>
        /// Used to start a poller that checks for changes in the cluster client configuration
        /// </summary>
        internal void StartPoller()
        {
            _config.Pool.UpdateLocator(new List<DnsEndPoint>(new[] { _endPoint }));
            _poller = new ConfigurationPoller(_config);
            _poller.StartTimer();
        }

        /// <summary>
        /// Used to start a poller that checks for changes in the cluster client configuration
        /// </summary>
        /// <param name="intervalDelay">Time between pollings, in miliseconds</param>
        internal void StartPoller(int intervalDelay)
        {
            _poller = new ConfigurationPoller(_config, intervalDelay);
            _poller.StartTimer();
        }

        #endregion

        #region Config Info
        /// <summary>
        /// Parses the string NodeConfig into a list of IPEndPoints for configuration
        /// </summary>
        /// <returns>A list of IPEndPoints for config to use</returns>
        internal List<DnsEndPoint> GetEndPointList()
        {
            try
            {
                var endpoints = AddrUtil.HashEndPointList(GetNodeConfig());

                lock (_nodesLock)
                {
                    var nodesToRemove = new HashSet<IMemcachedNode>();
                    foreach (var node in _nodes)
                    {
                        if (!endpoints.Contains(node.EndPoint))
                            nodesToRemove.Add(node);
                    }
                    foreach (var node in nodesToRemove)
                    {
                        _nodes.Remove(node);
                    }

                    foreach (var point in endpoints)
                    {
                        if (_nodes.FirstOrDefault(x => x.EndPoint.Equals(point)) == null)
                        {
                            _nodes.Add(_config.NodeFactory.CreateNode(point, _config.SocketPool, _config.LoggerFactory));
                        }
                    }
                }

                return endpoints;
            }
            catch (Exception ex)
            {
                // Error getting the list of endpoints. Most likely this is due to the
                // client being used outside of EC2. 
                _log.LogError(ex, "Error getting endpoints list");
                throw;
            }
        }

        /// <summary>
        /// Gets the Node configuration from "config get cluster" if it's new or "get AmazonElastiCache:cluster" if it's older than
        /// 1.4.14
        /// </summary>
        /// <returns>A string in the format "hostname1|ip1|port1 hostname2|ip2|port2 ..."</returns>
        internal string GetNodeConfig()
        {
            var tries = _tries;
            var nodeVersion = GetNodeVersion();
            var older = new Version("1.4.14");
            var waiting = true;
            string message = "";
            string[] items = null;

            var command = nodeVersion.CompareTo(older) < 0
                ? (IGetOperation) new GetOperation("AmazonElastiCache:cluster", _config.LoggerFactory.CreateLogger<GetOperation>())
                : new ConfigGetOperation("cluster", _config.LoggerFactory.CreateLogger<ConfigGetOperation>());

            while (waiting && tries > 0)
            {
                tries--;
                try
                {
                    lock (_nodesLock)
                    {
                        // This avoids timing out from requesting the config from the endpoint
                        foreach (var node in _nodes.ToArray())
                        {
                            try
                            {
                                var result = node.Execute(command);

                                if (result.Success)
                                {
                                    var configCommand = command as IConfigOperation;
                                    items = Encoding.UTF8.GetString(configCommand.ConfigResult.Data.Array, configCommand.ConfigResult.Data.Offset, configCommand.ConfigResult.Data.Count).Split('\n');
                                    waiting = false;
                                    break;
                                }

                                message = result.Message;
                            }
                            catch (Exception ex)
                            {
                                message = ex.Message;
                            }
                        }
                    }

                    if (waiting)
                        Thread.Sleep(_delay);

                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    Thread.Sleep(_delay);
                }
            }

            if (waiting)
            {
                throw new TimeoutException(string.Format("Could not get config of version " + NodeVersion + ". Tries: {0} Delay: {1}. " + message, _tries, _delay));
            }

            lock (_clusterLock)
            {
                if (ClusterVersion < Convert.ToInt32(items[0]))
                    ClusterVersion = Convert.ToInt32(items[0]);
            }
            return items[1];
        }

        /// <summary>
        /// Finds the version of Memcached the Elasticache setup is running on
        /// </summary>
        /// <returns>Version of memcahced running on nodes</returns>
        internal Version GetNodeVersion()
        {
            if (NodeVersion != null)
            {
                return NodeVersion;
            }

            #if DEBUG // For LocalSimulationTester
            if (!string.IsNullOrEmpty(_node.ToString()) && _node.ToString().Equals("TestingAWSInternal"))
            {
                NodeVersion = new Version("1.4.14");
                return NodeVersion;
            }
            #endif

            IStatsOperation statCommand = new StatsOperation(null);
            /* var statResult = */_node.Execute(statCommand);

            if (statCommand.Result != null && statCommand.Result.TryGetValue("version", out var version))
            {
                NodeVersion = new Version(version);
                return NodeVersion;
            }

            _log.LogError("Could not call stats on Node endpoint");
            throw new CommandNotSupportedException("The node does not have a version in stats.");
        }

        /// <summary>
        /// Tries to resolve the endpoint ip, used if the connection fails
        /// </summary>
        /// <returns>The resolved endpoint as an ip and port</returns>
        internal DnsEndPoint ResolveEndPoint()
        {
            IPHostEntry entry = null;
            var waiting = true;
            var tryCount = _tries;
            string message = "";

            while (tryCount > 0 && waiting)
            {
                try
                {
                    tryCount--;
                    entry = Dns.GetHostEntry(_hostname);
                    if (entry.AddressList.Length > 0)
                    {
                        waiting = false;
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    Thread.Sleep(_delay);
                }
            }


            if (waiting || entry == null)
            {
                _log.LogError("Could not resolve hostname to ip");
                throw new TimeoutException(string.Format("Could not resolve hostname to Ip after trying the specified amount: {0}. " + message, _tries));
            }

            _log.LogDebug("Resolved configuration endpoint {Hostname} to {Address}.", _hostname, entry.AddressList[0]);

            lock (_endpointLock)
            {
                _endPoint = new DnsEndPoint(entry.AddressList[0].ToString(), _port);
            }

            lock (_nodesLock)
            {
                if (_node != null)
                {
                    try
                    {
                        _node.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }
                }
                _node = _config.NodeFactory.CreateNode(_endPoint, _config.SocketPool, _config.LoggerFactory);
                _nodes.Clear();
                _nodes.Add(_node);
            }
            return _endPoint;
        }

        #endregion

        /// <summary>
        /// Stops the current poller
        /// </summary>
        public void Dispose()
        {
            _poller?.StopPolling();
            try
            {
                _nodes.ForEach(n => n.Dispose());
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on disposing nodes");
            }
            _nodes.Clear();
        }
    }
}
