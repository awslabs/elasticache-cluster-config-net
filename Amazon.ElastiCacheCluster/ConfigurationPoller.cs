using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Amazon.ElastiCacheCluster
{
    /// <summary>
    /// A poller used to reconfigure the client servers when updates occur to the cluster configuration
    /// </summary>
    internal class ConfigurationPoller
    {
        private static readonly Enyim.Caching.ILog log = Enyim.Caching.LogManager.GetLogger(typeof(ConfigurationPoller));

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

            this.timer = new Timer(this.intervalDelay);
            this.timer.Elapsed += this.pollOnTimedEvent;
        }

        #endregion

        #region Polling Methods

        internal void StartTimer()
        {
            log.Debug("Starting timer");
            this.pollOnTimedEvent(null, null);
            this.timer.Start();
        }

        /// <summary>
        /// Used by the poller's timer to update the cluster configuration if a new version is available
        /// </summary>
        internal void pollOnTimedEvent(Object source, ElapsedEventArgs e)
        {
            log.Debug("Polling...");
            try
            {
                var oldVersion = config.DiscoveryNode.ClusterVersion;
                var endPoints = config.DiscoveryNode.GetEndPointList();
                if (oldVersion != config.DiscoveryNode.ClusterVersion)
                {
                    this.config.Pool.UpdateLocator(endPoints);
                }
            }
            catch
            {
                try
                {
                    config.DiscoveryNode.ResolveEndPoint();

                    var oldVersion = config.DiscoveryNode.ClusterVersion;
                    var endPoints = config.DiscoveryNode.GetEndPointList();
                    if (oldVersion != config.DiscoveryNode.ClusterVersion)
                    {
                        this.config.Pool.UpdateLocator(endPoints);
                    }
                }
                catch (Exception ex)
                {
                    throw new TimeoutException("Could not retrieve cluster configuration after updating endpoint. " + ex.Message);
                }
            }
        }

        #endregion

        /// <summary>
        /// Disposes the background thread that is used for polling the configs
        /// </summary>
        public void StopPolling()
        {
            log.Debug("Destroying poller thread");
            if (this.timer != null)
                this.timer.Dispose();
        }
    }
}
