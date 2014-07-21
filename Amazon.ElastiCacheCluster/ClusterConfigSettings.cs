using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Amazon.ElastiCacheCluster.Factories;

namespace Amazon.ElastiCacheCluster
{
    /// <summary>
    /// A config settings object used to configure the client config
    /// </summary>
    public class ClusterConfigSettings : ConfigurationSection
    {
        /// <summary>
        /// An object that produces nodes for the Discovery Node, mainly used for testing
        /// </summary>
        public IConfigNodeFactory NodeFactory { get; set; }

        #region Constructors

        /// <summary>
        /// For config manager
        /// </summary>
        public ClusterConfigSettings() { }

        /// <summary>
        /// Used to initialize a setup with a host and port
        /// </summary>
        /// <param name="hostname">Cluster hostname</param>
        /// <param name="port">Cluster port</param>
        public ClusterConfigSettings(string hostname, int port)
        {
            if (string.IsNullOrEmpty(hostname))
                throw new ArgumentNullException("hostname");
            if (port <= 0)
                throw new ArgumentException("Port cannot be less than or equal to zero");

            this.ClusterEndPoint.HostName = hostname;
            this.ClusterEndPoint.Port = port;
        }

        #endregion

        #region Config Settings

        /// <summary>
        /// Class containing information about the cluster host and port
        /// </summary>
        [ConfigurationProperty("endpoint", IsRequired = true)]
        public Endpoint ClusterEndPoint
        {
            get { return (Endpoint)base["endpoint"]; }
            set { base["endpoint"] = value; }
        }

        /// <summary>
        /// Class containing information about the node configuration
        /// </summary>
        [ConfigurationProperty("node", IsRequired = false)]
        public NodeSettings ClusterNode
        {
            get { return (NodeSettings)base["node"]; }
            set { base["node"] = value; }
        }

        /// <summary>
        /// Class containing information about the poller configuration
        /// </summary>
        [ConfigurationProperty("poller", IsRequired = false)]
        public PollerSettings ClusterPoller
        {
            get { return (PollerSettings)base["poller"]; }
            set { base["poller"] = value; }
        }

        /// <summary>
        /// Endpoint that contains the hostname and port for auto discovery
        /// </summary>
        public class Endpoint : ConfigurationElement
        {
            /// <summary>
            /// The hostname of the cluster containing ".cfg."
            /// </summary>
            [ConfigurationProperty("hostname", IsRequired = true)]
            public String HostName
            {
                get
                {
                    return (String)this["hostname"];
                }
                set
                {
                    this["hostname"] = value;
                }
            }

            /// <summary>
            /// The port of the endpoint
            /// </summary>
            [ConfigurationProperty("port", IsRequired = true)]
            public int Port
            {
                get
                {
                    return (int)this["port"];
                }
                set
                {
                    this["port"] = value;
                }
            }
        }

        /// <summary>
        /// Settings used for the discovery node
        /// </summary>
        public class NodeSettings : ConfigurationElement
        {
            /// <summary>
            /// How many tries the node should use to get a config
            /// </summary>
            [ConfigurationProperty("nodeTries", DefaultValue = -1, IsRequired = false)]
            public int NodeTries
            {
                get { return (int)base["nodeTries"]; }
                set { base["nodeTries"] = value; }
            }

            /// <summary>
            /// The delay between tries for the config in miliseconds
            /// </summary>
            [ConfigurationProperty("nodeDelay", DefaultValue = -1, IsRequired = false)]
            public int NodeDelay
            {
                get { return (int)base["nodeDelay"]; }
                set { base["nodeDelay"] = value; }
            }
        }

        /// <summary>
        /// Settins used for the configuration poller
        /// </summary>
        public class PollerSettings : ConfigurationElement
        {
            /// <summary>
            /// The delay between polls in miliseconds
            /// </summary>
            [ConfigurationProperty("intervalDelay", DefaultValue = -1, IsRequired = false)]
            public int IntervalDelay
            {
                get { return (int)base["intervalDelay"]; }
                set { base["intervalDelay"] = value; }
            }
        }

        #endregion

        #region MemcachedConfig

        /// <summary>
        /// Gets or sets the configuration of the socket pool.
        /// </summary>
        [ConfigurationProperty("socketPool", IsRequired = false)]
        public SocketPoolElement SocketPool
        {
            get { return (SocketPoolElement)base["socketPool"]; }
            set { base["socketPool"] = value; }
        }

        /// <summary>
        /// Gets or sets the configuration of the authenticator.
        /// </summary>
        [ConfigurationProperty("authentication", IsRequired = false)]
        public AuthenticationElement Authentication
        {
            get { return (AuthenticationElement)base["authentication"]; }
            set { base["authentication"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.IMemcachedNodeLocator"/> which will be used to assign items to Memcached nodes.
        /// </summary>
        [ConfigurationProperty("locator", IsRequired = false)]
        public ProviderElement<IMemcachedNodeLocator> NodeLocator
        {
            get { return (ProviderElement<IMemcachedNodeLocator>)base["locator"]; }
            set { base["locator"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.IMemcachedKeyTransformer"/> which will be used to convert item keys for Memcached.
        /// </summary>
        [ConfigurationProperty("keyTransformer", IsRequired = false)]
        public ProviderElement<IMemcachedKeyTransformer> KeyTransformer
        {
            get { return (ProviderElement<IMemcachedKeyTransformer>)base["keyTransformer"]; }
            set { base["keyTransformer"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.ITranscoder"/> which will be used serialzie or deserialize items.
        /// </summary>
        [ConfigurationProperty("transcoder", IsRequired = false)]
        public ProviderElement<ITranscoder> Transcoder
        {
            get { return (ProviderElement<ITranscoder>)base["transcoder"]; }
            set { base["transcoder"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.IPerformanceMonitor"/> which will be used monitor the performance of the client.
        /// </summary>
        [ConfigurationProperty("performanceMonitor", IsRequired = false)]
        public ProviderElement<IPerformanceMonitor> PerformanceMonitor
        {
            get { return (ProviderElement<IPerformanceMonitor>)base["performanceMonitor"]; }
            set { base["performanceMonitor"] = value; }
        }

        /// <summary>
        /// Gets or sets the type of the communication between client and server.
        /// </summary>
        [ConfigurationProperty("protocol", IsRequired = false, DefaultValue = MemcachedProtocol.Binary)]
        public MemcachedProtocol Protocol
        {
            get { return (MemcachedProtocol)base["protocol"]; }
            set { base["protocol"] = value; }
        }

        #endregion

    }
}
