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
using System.Text;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Protocol;
using System.Net;
using System.Threading.Tasks;
using Amazon.ElastiCacheCluster.Operations;
using Enyim.Caching.Memcached.Results;

namespace LocalSimulationTests
{
    public class TestNode : IMemcachedNode
    {
        private EndPoint end;
        public EndPoint EndPoint { get { return end; } set { this.end = value; } }

        private int requestNum = 1;

        public Enyim.Caching.Memcached.Results.IOperationResult Execute(IOperation op)
        {
            IConfigOperation getOp = op as IConfigOperation;

            byte[] bytes;

            switch (requestNum)
            {
                case 1:
                    bytes = Encoding.UTF8.GetBytes(String.Format("{0}\r\ncluster.0001.use1.cache.amazon.aws.com|10.10.10.1|11211 cluster.0002.use1.cache.amazon.aws.com|10.10.10.2|11211 cluster.0003.use1.cache.amazon.aws.com|10.10.10.3|11211\r\n", this.requestNum));
                    break;
                case 2:
                    bytes = Encoding.UTF8.GetBytes(String.Format("{0}\r\ncluster.0002.use1.cache.amazon.aws.com|10.10.10.2|11211 cluster.0003.use1.cache.amazon.aws.com|10.10.10.3|11211\r\n", this.requestNum));
                    break;
                default:
                    bytes = Encoding.UTF8.GetBytes(String.Format("{0}\r\ncluster.0001.use1.cache.amazon.aws.com|10.10.10.1|11211\r\n", this.requestNum));
                    break;
            }
            this.requestNum++;

            var arr = new ArraySegment<byte>(bytes);
            getOp.ConfigResult = new CacheItem(0, arr);

            var result = new PooledSocketResult();
            result.Success = true;
            return result;
        }

        public Task<IOperationResult> ExecuteAsync(IOperation op)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "TestingAWSInternal";
        }

        public bool ExecuteAsync(IOperation op, Action<bool> next)
        {
            throw new NotImplementedException();
        }

        public event Action<IMemcachedNode> Failed;

        public bool IsAlive
        {
            get { throw new NotImplementedException(); }
        }

        public bool Ping()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
