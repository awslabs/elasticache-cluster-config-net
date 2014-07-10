using System;
using System.Collections.Generic;
using System.Net;
using Enyim.Caching.Memcached;
using Enyim.Reflection;
using Enyim.Caching.Memcached.Protocol.Binary;
using Enyim.Caching.Configuration;
using ElastiCacheCluster.Pools;
using ElastiCacheCluster.Factories;
using System.Configuration;

namespace ElastiCacheCluster
{
    /// <summary>
    /// Configuration class for auto discovery
    /// </summary>
    public class ElastiCacheClusterConfig : IMemcachedClientConfiguration
    {
        internal static readonly ClusterConfigSettings DefaultSettings = ConfigurationManager.GetSection("clusterclient") as ClusterConfigSettings;

        // these are lazy initialized in the getters
        private Type nodeLocator;
        private ITranscoder transcoder;
        private IMemcachedKeyTransformer keyTransformer;

        internal ClusterConfigSettings setup;
        internal AutoServerPool Pool;
        internal IConfigNodeFactory nodeFactory;

        /// <summary>
        /// The node used to check the cluster's configuration
        /// </summary>
        public DiscoveryNode DiscoveryNode { get; private set; }

        /// <summary>
        /// Initializes a MemcahcedClient config with auto discovery enabled from the app.config clusterclient section
        /// </summary>
        public ElastiCacheClusterConfig()
            : this(DefaultSettings) { }

        /// <summary>
        /// Initializes a MemcahcedClient config with auto discovery enabled from the app.config with the specified section
        /// </summary>
        /// <param name="section">The section to get config settings from</param>
        public ElastiCacheClusterConfig(string section)
            : this(ConfigurationManager.GetSection(section) as ClusterConfigSettings) { }

        /// <summary>
        /// Initializes a MemcahcedClient config with auto discovery enabled
        /// </summary>
        /// <param name="hostname">The hostname of the cluster containing ".cfg."</param>
        /// <param name="port">The port to connect to for communication</param>
        public ElastiCacheClusterConfig(string hostname, int port)
            : this(new ClusterConfigSettings(hostname, port)) { }

        /// <summary>
        /// Initializes a MemcahcedClient config with auto discovery enabled using the setup provided
        /// </summary>
        /// <param name="setup">The setup to get conifg settings from</param>
        public ElastiCacheClusterConfig(ClusterConfigSettings setup)
        {
            if (String.IsNullOrEmpty(setup.ClusterEndPoint.HostName))
                throw new ArgumentNullException("hostname");
            if (setup.ClusterEndPoint.Port <= 0)
                throw new ArgumentException("Port cannot be 0 or less");

            this.setup = setup;
            this.Servers = new List<IPEndPoint>();

            this.Protocol = setup.Protocol;
            this.KeyTransformer = setup.KeyTransformer.CreateInstance() ?? new DefaultKeyTransformer();
            this.SocketPool = (ISocketPoolConfiguration)setup.SocketPool ?? new SocketPoolConfiguration();
            this.Authentication = (IAuthenticationConfiguration)setup.Authentication ?? new AuthenticationConfiguration();

            this.nodeFactory = setup.NodeFactory ?? new DefaultConfigNodeFactory();

            if (setup.ClusterEndPoint.HostName.Contains(".cfg."))
            {
                if (setup.ClusterNode.NodeTries < 0 || setup.ClusterNode.NodeDelay < 0)
                    this.DiscoveryNode = new DiscoveryNode(this, setup.ClusterEndPoint.HostName, setup.ClusterEndPoint.Port);
                else
                    this.DiscoveryNode = new DiscoveryNode(this, setup.ClusterEndPoint.HostName, setup.ClusterEndPoint.Port, setup.ClusterNode.NodeTries, setup.ClusterNode.NodeDelay);
            }
            else
            {
                throw new ArgumentException("The provided endpoint does not support auto discovery");
            }
        }

        #region Members

        /// <summary>
        /// Gets a list of <see cref="T:IPEndPoint"/> each representing a Memcached server in the pool.
        /// </summary>
        public IList<IPEndPoint> Servers { get; private set; }

        /// <summary>
        /// Gets the configuration of the socket pool.
        /// </summary>
        public ISocketPoolConfiguration SocketPool { get; private set; }

        /// <summary>
        /// Gets the authentication settings.
        /// </summary>
        public IAuthenticationConfiguration Authentication { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.IMemcachedKeyTransformer"/> which will be used to convert item keys for Memcached.
        /// </summary>
        public IMemcachedKeyTransformer KeyTransformer
        {
            get { return this.keyTransformer ?? (this.keyTransformer = new DefaultKeyTransformer()); }
            set { this.keyTransformer = value; }
        }

        /// <summary>
        /// Gets or sets the Type of the <see cref="T:Enyim.Caching.Memcached.IMemcachedNodeLocator"/> which will be used to assign items to Memcached nodes.
        /// </summary>
        /// <remarks>If both <see cref="M:NodeLocator"/> and  <see cref="M:NodeLocatorFactory"/> are assigned then the latter takes precedence.</remarks>
        public Type NodeLocator
        {
            get { return this.nodeLocator; }
            set
            {
                ConfigurationHelper.CheckForInterface(value, typeof(IMemcachedNodeLocator));
                this.nodeLocator = value;
            }
        }

        /// <summary>
        /// Gets or sets the NodeLocatorFactory instance which will be used to create a new IMemcachedNodeLocator instances.
        /// </summary>
        /// <remarks>If both <see cref="M:NodeLocator"/> and  <see cref="M:NodeLocatorFactory"/> are assigned then the latter takes precedence.</remarks>
        public IProviderFactory<IMemcachedNodeLocator> NodeLocatorFactory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.ITranscoder"/> which will be used serialize or deserialize items.
        /// </summary>
        public ITranscoder Transcoder
        {
            get { return this.transcoder ?? (this.transcoder = new DefaultTranscoder()); }
            set { this.transcoder = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.IPerformanceMonitor"/> instance which will be used monitor the performance of the client.
        /// </summary>
        public IPerformanceMonitor PerformanceMonitor { get; set; }

        /// <summary>
        /// Gets or sets the type of the communication between client and server.
        /// </summary>
        public MemcachedProtocol Protocol { get; set; }

        #endregion

        #region [ interface                     ]

        IList<System.Net.IPEndPoint> IMemcachedClientConfiguration.Servers
        {
            get { return this.Servers; }
        }

        ISocketPoolConfiguration IMemcachedClientConfiguration.SocketPool
        {
            get { return this.SocketPool; }
        }

        IAuthenticationConfiguration IMemcachedClientConfiguration.Authentication
        {
            get { return this.Authentication; }
        }

        IMemcachedKeyTransformer IMemcachedClientConfiguration.CreateKeyTransformer()
        {
            return this.KeyTransformer;
        }

        IMemcachedNodeLocator IMemcachedClientConfiguration.CreateNodeLocator()
        {
            var f = this.NodeLocatorFactory;
            if (f != null) return f.Create();

            return this.NodeLocator == null
                    ? new DefaultNodeLocator()
                    : (IMemcachedNodeLocator)FastActivator.Create(this.NodeLocator);
        }

        ITranscoder IMemcachedClientConfiguration.CreateTranscoder()
        {
            return this.Transcoder;
        }

        IServerPool IMemcachedClientConfiguration.CreatePool()
        {
            switch (this.Protocol)
            {
                case MemcachedProtocol.Text: 
                    this.Pool = new AutoServerPool(this, new Enyim.Caching.Memcached.Protocol.Text.TextOperationFactory());
                    break;
                case MemcachedProtocol.Binary: 
                    this.Pool = new AutoBinaryPool(this);
                    break;
                default:                    
                    throw new ArgumentOutOfRangeException("Unknown protocol: " + (int)this.Protocol);
            }

            if (setup.ClusterPoller.IntervalDelay < 0)
                this.DiscoveryNode.StartPoller();
            else
                this.DiscoveryNode.StartPoller(setup.ClusterPoller.IntervalDelay);
            
            return this.Pool;
        }

        IPerformanceMonitor IMemcachedClientConfiguration.CreatePerformanceMonitor()
        {
            return this.PerformanceMonitor;
        }

        #endregion
    }
}
