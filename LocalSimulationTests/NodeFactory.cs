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
using Amazon.ElastiCacheCluster.Factories;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Microsoft.Extensions.Logging;

namespace LocalSimulationTests
{
    public class NodeFactory : IConfigNodeFactory
    {
        TestNode node;

        public NodeFactory()
        {
            this.node = new TestNode();
        }

        public IMemcachedNode CreateNode(DnsEndPoint endpoint, ISocketPoolConfiguration config, ILoggerFactory loggerFactory)
        {
            node.EndPoint = endpoint;
            return node;
        }
    }
}
