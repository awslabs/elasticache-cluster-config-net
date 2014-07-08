using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Protocol;
using Enyim.Caching.Memcached.Results;
using Enyim.Caching.Memcached.Results.Extensions;
using ElastiCacheCluster.Helpers;

namespace ElastiCacheCluster.Operations
{
	internal class GetOperation : SingleItemOperation, IGetOperation
	{
		private CacheItem result;

		internal GetOperation(string key) : base(key) { }

		protected override System.Collections.Generic.IList<System.ArraySegment<byte>> GetBuffer()
		{
			var command = "gets " + this.Key + TextSocketHelper.CommandTerminator;

			return TextSocketHelper.GetCommandBuffer(command);
		}

		protected override IOperationResult ReadResponse(PooledSocket socket)
		{
			GetResponse r = GetHelper.ReadItem(socket);
			var result = new TextOperationResult();

			if (r == null) return result.Fail("Failed to read response");

			this.result = r.Item;
			this.Cas = r.CasValue;

			GetHelper.FinishCurrent(socket);

			return result.Pass();
		}

		CacheItem IGetOperation.Result
		{
			get { return this.result; }
		}

		protected override bool ReadResponseAsync(PooledSocket socket, System.Action<bool> next)
		{
			throw new System.NotSupportedException();
		}
	}
}