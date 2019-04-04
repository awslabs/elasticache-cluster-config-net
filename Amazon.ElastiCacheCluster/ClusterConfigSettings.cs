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
using System.ComponentModel.DataAnnotations;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Amazon.ElastiCacheCluster.Factories;

namespace Amazon.ElastiCacheCluster
{
    /// <summary>
    /// A config settings object used to configure the client config
    /// </summary>
    public class ClusterConfigSettings
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
                throw new ArgumentNullException(nameof(hostname));
            if (port <= 0)
                throw new ArgumentException("Port cannot be less than or equal to zero");

            ClusterEndPoint = new Endpoint
            {
                HostName = hostname,
                Port = port,
            };
        }

        #endregion

        #region Config Settings

        /// <summary>
        /// Class containing information about the cluster host and port
        /// </summary>
        [Required]
        public Endpoint ClusterEndPoint { get; set; }

        /// <summary>
        /// Class containing information about the node configuration
        /// </summary>
        public NodeSettings ClusterNode { get; set; }

        /// <summary>
        /// Class containing information about the poller configuration
        /// </summary>
        public PollerSettings ClusterPoller { get; set; } = new PollerSettings();

        /// <summary>
        /// Endpoint that contains the hostname and port for auto discovery
        /// </summary>
        public class Endpoint 
        {
            /// <summary>
            /// The hostname of the cluster containing ".cfg."
            /// </summary>
            [Required]
            public String HostName { get; set; }

            /// <summary>
            /// The port of the endpoint
            /// </summary>
            [Required]
            public int Port { get; set; }
        }

        /// <summary>
        /// Settings used for the discovery node
        /// </summary>
        public class NodeSettings
        {
            /// <summary>
            /// How many tries the node should use to get a config
            /// </summary>
            public int NodeTries { get; set; } = -1;

            /// <summary>
            /// The delay between tries for the config in miliseconds
            /// </summary>
            public int NodeDelay { get; set; } = -1;
        }

        /// <summary>
        /// Settins used for the configuration poller
        /// </summary>
        public class PollerSettings
        {
            /// <summary>
            /// The delay between polls in miliseconds
            /// </summary>
            public int IntervalDelay { get; set; } = -1;
        }

        #endregion

        #region MemcachedConfig

        /// <summary>
        /// Gets or sets the configuration of the socket pool.
        /// </summary>
        public ISocketPoolConfiguration SocketPool { get; set; }

        /// <summary>
        /// Gets or sets the configuration of the authenticator.
        /// </summary>
        public IAuthenticationConfiguration Authentication { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.IMemcachedNodeLocator"/> which will be used to assign items to Memcached nodes.
        /// </summary>
        public Type NodeLocator { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.IMemcachedKeyTransformer"/> which will be used to convert item keys for Memcached.
        /// </summary>
        public IMemcachedKeyTransformer KeyTransformer { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.ITranscoder"/> which will be used serialzie or deserialize items.
        /// </summary>
        public ITranscoder Transcoder { get; set; }

        /// <summary>
        /// Gets or sets the type of the communication between client and server.
        /// </summary>
        public MemcachedProtocol Protocol { get; set; } = MemcachedProtocol.Binary;

        #endregion

    }
}
