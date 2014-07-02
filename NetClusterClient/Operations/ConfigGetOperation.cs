using Enyim.Caching.Memcached.Protocol;
using Enyim.Caching.Memcached.Protocol.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Enyim.Caching.Memcached.Results;
using Enyim.Caching.Memcached.Results.Extensions;
using Enyim.Caching.Memcached;
using NetClusterClient.Helpers;

namespace NetClusterClient
{
    /// <summary>
    /// Used to get auto discovery information from ElastiCache endpoints
    /// </summary>
    public class ConfigGetOperation : SingleItemOperation, IGetOperation
    {
        private CacheItem result;

        /// <summary>
        /// Creates a config get for ElastiCache
        /// </summary>
        /// <param name="key"></param>
		public ConfigGetOperation(string key) : base(key) { }

        protected override IList<ArraySegment<byte>> GetBuffer()
        {
            var command = "config get " + this.Key + TextSocketHelper.CommandTerminator;

            return TextSocketHelper.GetCommandBuffer(command);
        }

        protected override Enyim.Caching.Memcached.Results.IOperationResult ReadResponse(PooledSocket socket)
        {
            string description = TextSocketHelper.ReadResponse(socket);

            if (String.Compare(description, "END", StringComparison.Ordinal) == 0)
                return null;

            if (description.Length < 7 || String.Compare(description, 0, "CONFIG ", 0, 7, StringComparison.Ordinal) != 0)
                throw new MemcachedClientException("No CONFIG response received.\r\n" + description);

            string[] parts = description.Split(' ');

            /****** Format ********
             *
             * CONFIG <key> <flags> <bytes>
             * 0        1       2       3
             * 
             */

            ushort flags = UInt16.Parse(parts[2], CultureInfo.InvariantCulture);
            int length = Int32.Parse(parts[3], CultureInfo.InvariantCulture);

            byte[] allNodes = new byte[length];
            byte[] eod = new byte[2];

            socket.Read(allNodes, 0, length);
            socket.Read(eod, 0, 2); // data is terminated by \r\n

            this.result = new CacheItem(flags, new ArraySegment<byte>(allNodes, 0, length));

            string response = TextSocketHelper.ReadResponse(socket);

            if (String.Compare(response, "END", StringComparison.Ordinal) != 0)
                throw new MemcachedClientException("No END was received.");

            var result = new TextOperationResult();
            return result.Pass();
        }

        protected override bool ReadResponseAsync(PooledSocket socket, Action<bool> next)
        {
            throw new System.NotSupportedException();
        }

        /// <summary>
        /// The CacheItem result of a "get config *key*" request
        /// </summary>
        public CacheItem Result
        {
            get { return result; }
        }
    }
}
