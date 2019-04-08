/*
 * Copyright 2014 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Portions copyright 2010 Attila Kisk�, enyim.com. Please see LICENSE.txt
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.ElastiCacheCluster.Helpers;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Protocol;
using Enyim.Caching.Memcached.Results;
using Enyim.Caching.Memcached.Results.Extensions;
using Microsoft.Extensions.Logging;

namespace Amazon.ElastiCacheCluster.Operations
{
    internal class GetOperation : SingleItemOperation, IGetOperation, IConfigOperation
    {
        private CacheItem _result;
        private readonly ILogger _log;

        internal GetOperation(string key, ILogger log) : base(key)
        {
            _log = log;
        }

        protected override IList<ArraySegment<byte>> GetBuffer()
        {
            var command = "gets " + Key + TextSocketHelper.CommandTerminator;

            return TextSocketHelper.GetCommandBuffer(command);
        }

        protected override IOperationResult ReadResponse(PooledSocket socket)
        {
            GetResponse r = GetHelper.ReadItem(socket, _log);
            var result = new TextOperationResult();

            if (r == null) return result.Fail("Failed to read response");

            _result = r.Item;
            ConfigResult = r.Item;

            Cas = r.CasValue;

            GetHelper.FinishCurrent(socket, _log);

            return result.Pass();
        }

        protected override ValueTask<IOperationResult> ReadResponseAsync(PooledSocket socket)
        {
            return new ValueTask<IOperationResult>(ReadResponse(socket));
        }

        CacheItem IGetOperation.Result
        {
            get { return _result; }
        }

        protected override bool ReadResponseAsync(PooledSocket socket, Action<bool> next)
        {
            throw new NotSupportedException();
        }

        public CacheItem ConfigResult { get; set; }
    }
}
