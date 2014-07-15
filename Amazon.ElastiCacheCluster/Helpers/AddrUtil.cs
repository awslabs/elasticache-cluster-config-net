using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Amazon.ElastiCacheCluster.Helpers
{
    /// <summary>
    /// A class used to parse configs of Auto Discovery
    /// </summary>
    internal static class AddrUtil
    {
        /// <summary>
        /// Creates a list of endpoints from a string returned in the config request
        /// </summary>
        /// <param name="endpoints">Format: host1|ip1|port1 host2|ip2|port2 ...</param>
        /// <returns>List of the endpoints parsed to ip:port endpoints for connections</returns>
        public static List<IPEndPoint> HashEndPointList(string endpoints)
        {
            List<IPEndPoint> list = new List<IPEndPoint>();
            foreach (var node in endpoints.Split(' '))
            {
                string[] parts = node.Split('|');
                IPAddress ip;
                if (IPAddress.TryParse(parts[1], out ip))
                {
                    list.Add(new IPEndPoint(ip, Convert.ToInt32(parts[2])));
                }
            }
            return list;
        }
    }
}
