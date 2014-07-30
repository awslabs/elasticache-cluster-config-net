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
