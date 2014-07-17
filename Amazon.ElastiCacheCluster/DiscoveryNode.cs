using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Configuration;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Amazon.ElastiCacheCluster.Helpers;
using Amazon.ElastiCacheCluster.Operations;
using Amazon.ElastiCacheCluster.Pools;
using Enyim.Caching.Memcached.Results;
using Enyim.Caching.Memcached.Protocol;

namespace Amazon.ElastiCacheCluster
{
    /// <summary>
    /// A class that manages the discovery of endpoints inside of an ElastiCache cluster
    /// </summary>
    public class DiscoveryNode
    {
        #region Static ReadOnlys

        private static readonly Enyim.Caching.ILog log = Enyim.Caching.LogManager.GetLogger(typeof(DiscoveryNode));
      
        internal static readonly int DEFAULT_TRY_COUNT = 5;
        internal static readonly int DEFAULT_TRY_DELAY = 1000;

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
        public int NodesInCluster { get { return this.nodes.Count; } }

        #region Private Fields

        private IPEndPoint EndPoint;

        private IMemcachedNode Node;
        
        private ElastiCacheClusterConfig config;

        private List<IMemcachedNode> nodes = new List<IMemcachedNode>();

        private ConfigurationPoller poller;

        private string hostname;
        private int port;

        private int tries;
        private int delay;

        private Object nodesLock, endpointLock, clusterLock;

        #endregion

        #region Constructors

        /// <summary>
        /// The node used to discover endpoints in an ElastiCache cluster
        /// </summary>
        /// <param name="client">The client discovery node is contained within</param>
        /// <param name="hostname">The host name of the cluster with .cfg. in name</param>
        /// <param name="port">The port of the cluster</param>
        /// <param name="config">The config of the client to access the SocketPool</param>
        internal DiscoveryNode(ElastiCacheClusterConfig config, string hostname, int port)
            : this(config, hostname, port, DEFAULT_TRY_COUNT, DEFAULT_TRY_DELAY) { }

        /// <summary>
        /// The node used to discover endpoints in an ElastiCache cluster
        /// </summary>
        /// <param name="client">The client discovery node is contained within</param>
        /// <param name="hostname">The host name of the cluster with .cfg. in name</param>
        /// <param name="port">The port of the cluster</param>
        /// <param name="config">The config of the client to access the SocketPool</param>
        /// <param name="tries">The number of tries for requesting config info</param>
        /// <param name="delay">The time, in miliseconds, to wait between tries</param>
        internal DiscoveryNode(ElastiCacheClusterConfig config, string hostname, int port, int tries, int delay)
        {
            #region Param Checks

            if (config == null)
                throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(hostname))
                throw new ArgumentNullException("hostname");
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

            this.hostname = hostname;
            this.port = port;
            this.config = config;
            this.ClusterVersion = 0;
            this.tries = tries;
            this.delay = delay;

            this.clusterLock = new Object();
            this.endpointLock = new Object();
            this.nodesLock = new Object();

            #endregion

            this.ResolveEndPoint();
            this.GetNodeVersion();
        }

        #endregion

        #region Poller Methods

        /// <summary>
        /// Used to start a poller that checks for changes in the cluster client configuration
        /// </summary>
        internal void StartPoller()
        {
            this.config.Pool.UpdateLocator(new List<IPEndPoint>(new IPEndPoint[] { this.EndPoint }));
            this.poller = new ConfigurationPoller(this.config);
            this.poller.StartTimer();
        }

        /// <summary>
        /// Used to start a poller that checks for changes in the cluster client configuration
        /// </summary>
        /// <param name="intervalDelay">Time between pollings, in miliseconds</param>
        internal void StartPoller(int intervalDelay)
        {
            this.poller = new ConfigurationPoller(this.config, intervalDelay);
            this.poller.StartTimer();
        }

        #endregion

        #region Config Info

        /// <summary>
        /// Parses the string NodeConfig into a list of IPEndPoints for configuration
        /// </summary>
        /// <returns>A list of IPEndPoints for config to use</returns>
        internal List<IPEndPoint> GetEndPointList()
        {
            try
            {
                var endpoints = AddrUtil.HashEndPointList(this.GetNodeConfig());

                lock (nodesLock)
                {
                    this.nodes.Clear();

                    foreach (var point in endpoints)
                    {
                        this.nodes.Add(this.config.nodeFactory.CreateNode(point, this.config.SocketPool));
                    }
                }

                return endpoints;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the Node configuration from "config get cluster" if it's new or "get AmazonElastiCache:cluster" if it's older than
        /// 1.4.14
        /// </summary>
        /// <returns>A string in the format "hostname1|ip1|port1 hostname2|ip2|port2 ..."</returns>
        internal string GetNodeConfig()
        {
            var tries = this.tries;
            var nodeVersion = this.GetNodeVersion();
            var older = new Version("1.4.14");
            var waiting = true;
            string message = "";            
            string[] items = null;

            IGetOperation command = nodeVersion.CompareTo(older) < 0 ?                                        
                                        command = new GetOperation("AmazonElastiCache:cluster") :
                                        command = new ConfigGetOperation("cluster");

            while (waiting && tries > 0)
            {
                tries--;
                try
                {
                    lock (nodesLock)
                    {
                        // This avoids timing out from requesting the config from the endpoint
                        foreach (var node in this.nodes.ToArray())
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
                                else
                                {
                                    message = result.Message;
                                }
                            }
                            catch (Exception ex)
                            {
                                message = ex.Message;
                            }
                        }
                    }

                    if (waiting)
                        System.Threading.Thread.Sleep(this.delay);

                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    System.Threading.Thread.Sleep(this.delay);
                }
            }

            if (waiting)
            {
                throw new TimeoutException(String.Format("Could not get config of version " + this.NodeVersion.ToString() + ". Tries: {0} Delay: {1}. " + message, this.tries, this.delay));
            }

            lock (clusterLock)
            {
                if (this.ClusterVersion < Convert.ToInt32(items[0]))
                    this.ClusterVersion = Convert.ToInt32(items[0]);
            }
            return items[1];
        }

        /// <summary>
        /// Finds the version of Memcached the Elasticache setup is running on
        /// </summary>
        /// <returns>Version of memcahced running on nodes</returns>
        internal Version GetNodeVersion()
        {
            if (this.NodeVersion != null)
            {
                return this.NodeVersion;
            }

            if (!string.IsNullOrEmpty(this.Node.ToString()) && this.Node.ToString().Equals("TestingAWSInternal"))
            {
                this.NodeVersion = new Version("1.4.14");
                return this.NodeVersion;
            }

            IStatsOperation statcommand = new Enyim.Caching.Memcached.Protocol.Text.StatsOperation(null);
            var statresult = this.Node.Execute(statcommand);

            string version;
            if (statcommand.Result.TryGetValue("version", out version))
            {
                this.NodeVersion = new Version(version);
                return this.NodeVersion;
            }
            else
            {
                log.Error("Could not call stats on Node endpoint");
                throw new CommandNotSupportedException("The node does not have a version in stats.");
            }
        }

        /// <summary>
        /// Tries to resolve the endpoint ip, used if the connection fails
        /// </summary>
        /// <returns>The resolved endpoint as an ip and port</returns>
        internal IPEndPoint ResolveEndPoint()
        {
            IPHostEntry entry = null;
            var waiting = true;
            var tryCount = this.tries;
            string message = "";

            while (tryCount > 0 && waiting)
            {
                try
                {
                    tryCount--;
                    entry = Dns.GetHostEntry(hostname);
                    if (entry.AddressList.Length > 0)
                    {
                        waiting = false;
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    System.Threading.Thread.Sleep(this.delay);
                } 
            }

            if (waiting || entry == null)
            {
                log.Error("Could not resolve hostname to ip");
                throw new TimeoutException(String.Format("Could not resolve hostname to Ip after trying the specified amount: {0}. " + message, this.tries));
            }

            lock (endpointLock)
            {
                this.EndPoint = new IPEndPoint(entry.AddressList[0], port);
            }

            lock (nodesLock)
            {
                try
                {
                    this.Node.Dispose();
                }
                catch { }
                this.Node = this.config.nodeFactory.CreateNode(this.EndPoint, this.config.SocketPool);
                this.nodes.Clear();
                this.nodes.Add(this.Node);
            }
            return this.EndPoint;
        }

        #endregion

        /// <summary>
        /// Stops the current poller
        /// </summary>
        public void Dispose()
        {
            if (this.poller != null)
                this.poller.StopPolling();
        }
    }
}
