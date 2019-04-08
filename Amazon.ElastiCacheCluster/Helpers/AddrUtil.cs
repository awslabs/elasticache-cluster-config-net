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
using System.Net;

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
        public static List<DnsEndPoint> HashEndPointList(string endpoints)
        {
            var list = new List<DnsEndPoint>();
            foreach (var node in endpoints.Split(' '))
            {
                string[] parts = node.Split('|');
                if (IPAddress.TryParse(parts[1], out _))
                {
                    list.Add(new DnsEndPoint(parts[1], Convert.ToInt32(parts[2])));
                }
            }
            return list;
        }
    }
}
