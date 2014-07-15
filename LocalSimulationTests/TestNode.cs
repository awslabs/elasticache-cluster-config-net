using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Protocol;
using System.Net;
using Amazon.ElastiCacheCluster.Operations;
using Enyim.Caching.Memcached.Results;

namespace LocalSimulationTests
{
    public class TestNode : IMemcachedNode
    {
        private IPEndPoint end;
        public IPEndPoint EndPoint { get { return end; } }

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
