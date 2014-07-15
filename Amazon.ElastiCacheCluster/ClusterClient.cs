using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching;

namespace Amazon.ElastiCacheCluster
{
    /// <summary>
    /// Used to instantiate MemcachedClients with auto discovery enabled.
    /// Only use these for easy creation because the ability to get information from the config object is lost
    /// </summary>
    public static class ClusterClient
    {
        /// <summary>
        /// Creates a MemcachedClient using the settings found in the app.config section "clusterclient"
        /// </summary>
        /// <returns>A new MemcachedClient configured for auto discovery</returns>
        public static MemcachedClient CreateClient()
        {
            return new MemcachedClient(new ElastiCacheClusterConfig());
        }

        /// <summary>
        /// Creates a MemcachedClient using the settings found in the app.config section specified
        /// </summary>
        /// <param name="section">A section in app.config that has a endpoint field</param>
        /// <returns>A new MemcachedClient configured for auto discovery</returns>
        public static MemcachedClient CreateClient(string section)
        {
            return new MemcachedClient(new ElastiCacheClusterConfig(section));
        }

        /// <summary>
        /// Creates a MemcachedClient using the default settings with the endpoint and port specified
        /// </summary>
        /// <param name="endpoint">The url for the cluster endpoint containing .cfg.</param>
        /// <param name="port">The port to access the cluster on</param>
        /// <returns>A new MemcachedClient configured for auto discovery</returns>
        public static MemcachedClient CreateClient(string endpoint, int port)
        {
            return new MemcachedClient(new ElastiCacheClusterConfig(endpoint, port));
        }

        /// <summary>
        /// Creates a MemcachedClient using the Client config provided
        /// </summary>
        /// <param name="config">The config to instantiate the client with</param>
        /// <returns>A new MemcachedClient configured for auto discovery</returns>
        public static MemcachedClient CreateClient(ElastiCacheClusterConfig config)
        {
            return new MemcachedClient(config);
        }
    }
}
