using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Configuration;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using NetClusterClient.Helpers;
using NetClusterClient.Operations;
using NetClusterClient.Pools;
using Enyim.Caching.Memcached.Results;
using Enyim.Caching.Memcached.Protocol;

namespace NetClusterClient
{
    /// <summary>
    /// A class that manages the discover of endpoints inside of an ElastiCache cluster
    /// </summary>
    public class DiscoveryNode
    {
        private static readonly Enyim.Caching.ILog log = Enyim.Caching.LogManager.GetLogger(typeof(DiscoveryNode));
        
        private static readonly int DEFAULT_TRY_COUNT = 5;
        private static readonly int DEFAULT_TRY_DELAY = 1000;

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

        private IPEndPoint EndPoint;

        private IMemcachedNode Node;
        
        private AutoClientConfig config;

        private List<IMemcachedNode> nodes = new List<IMemcachedNode>();

        private ConfigurationPoller poller;

        private string hostname;
        private int port;

        private int tries;
        private int delay;
                
        /// <summary>
        /// The node used to discover endpoints in an ElastiCache cluster
        /// </summary>
        /// <param name="client">The client discovery node is contained within</param>
        /// <param name="hostname">The host name of the cluster with .cfg. in name</param>
        /// <param name="port">The port of the cluster</param>
        /// <param name="config">The config of the client to access the SocketPool</param>
        public DiscoveryNode(AutoClientConfig config, string hostname, int port)
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
        public DiscoveryNode(AutoClientConfig config, string hostname, int port, int tries, int delay)
        {            
            this.hostname = hostname;
            this.port = port;
            this.config = config;
            this.ClusterVersion = 0;
            this.tries = tries;
            this.delay = delay;

            this.ResolveEndPoint();
            
            this.nodes.Add(this.Node);
        }

        /// <summary>
        /// Used to start a poller that checks for changes in the cluster client configuration
        /// </summary>
        public void StartPoller()
        {
            this.poller = new ConfigurationPoller(this.config);
        }

        /// <summary>
        /// Used to start a poller that checks for changes in the cluster client configuration
        /// </summary>
        /// <param name="initialDelay">Time to wait, in miliseconds, before the first poll takes place</param>
        /// <param name="intervalDelay">Time between pollings, in miliseconds</param>
        public void StartPoller(int initialDelay, int intervalDelay)
        {
            this.poller = new ConfigurationPoller(this.config, initialDelay, intervalDelay);
        }

        /// <summary>
        /// Parses the string NodeConfig into a list of IPEndPoints for configuration
        /// </summary>
        /// <returns>A list of IPEndPoints for config to use</returns>
        public List<IPEndPoint> GetEndPointList()
        {
            try
            {
                var endpoints = AddrUtil.HashEndPointList(this.GetNodeConfig());
                this.nodes.Clear();

                foreach (var point in endpoints)
                {
                    this.nodes.Add(new MemcachedNode(point, this.config.SocketPool));
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
        public string GetNodeConfig()
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
                    // This avoids timing out from requesting the config from the endpoint
                    foreach (var node in this.nodes.ToArray())
                    {
                        try
                        {
                            var result = node.Execute(command);

                            if (result.Success)
                            {
                                items = Encoding.UTF8.GetString(command.Result.Data.Array, command.Result.Data.Offset, command.Result.Data.Count).Split('\n');
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

            this.ClusterVersion = Convert.ToInt32(items[0]);
            return items[1];
        }

        /// <summary>
        /// Finds the version of Memcached the Elasticache setup is running on
        /// </summary>
        /// <returns>Version of memcahced running on nodes</returns>
        public Version GetNodeVersion()
        {
            if (this.NodeVersion != null)
            {
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
        public IPEndPoint ResolveEndPoint()
        {
            IPHostEntry entry = null;
            var waiting = true;
            var tryCount = this.tries > 20 ? this.tries : 20;
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
                throw new TimeoutException(String.Format("Could not resolve hostname to Ip after trying the specified amount: {0}. " + message, this.tries > 20 ? this.tries : 20));
            }

            this.EndPoint = new IPEndPoint(entry.AddressList[0], port);
            try
            {
                this.Node.Dispose();
            }
            catch { }
            this.Node = new MemcachedNode(this.EndPoint, this.config.SocketPool);
            this.nodes.Clear();
            this.nodes.Add(this.Node);
            return this.EndPoint;
        }

        /// <summary>
        /// Stops the current poller
        /// </summary>
        public void Dispose()
        {
            this.poller.StopPolling();
        }
    }
}
