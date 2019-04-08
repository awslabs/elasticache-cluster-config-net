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
using System.Windows.Forms;
using Amazon.ElastiCacheCluster;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Microsoft.Extensions.Logging;

namespace ClusterClientAppTester
{
    public partial class Form1 : Form
    {
        private MemcachedClient _mem;
        private ElastiCacheClusterConfig _config;
        private readonly ILoggerFactory _loggerFactory;

        public Form1(ILoggerFactory loggerFactory, ElastiCacheClusterConfig config)
        {
            InitializeComponent();
            _loggerFactory = loggerFactory;
            _config = config;
        }

        public void ErrorAlert(string error)
        {
            MessageBox.Show(error);
        }

        private void ButtonInstantiate_Click(object sender, EventArgs e)
        {
            try
            {
                LabelStatus.Text = "Instantiating";
                _mem = new MemcachedClient(_loggerFactory, _config);

                #region UI Stuff

                TimerPoller.Enabled = false;
                ProgressPoller.Value = 0;
                TimerPoller.Enabled = true;

                if (_mem == null)
                {
                    LabelStatus.Text = "MemcachedClient returned a null object";
                }
                else
                {
                    ButtonAdd.Enabled = true;
                    ButtonGet.Enabled = true;
                    LabelStatus.Text = "Instantiation Success";
                    ProgressBarStatus.Value = 25;
                }

                #endregion
            }
            catch (Exception ex)
            {
                LabelStatus.Text = ex.Message;
            }
        }

        private void ButtonOlder_Click(object sender, EventArgs e)
        {
            try
            {
                LabelStatus.Text = "Instantiating";

                // Instantiates client with default settings and uses the hostname and port provided
                _config = new ElastiCacheClusterConfig(_loggerFactory, TextOlder.Text, Convert.ToInt32(TextPort.Text));

                _mem = new MemcachedClient(_loggerFactory, _config);

                #region UI Stuff

                TimerPoller.Enabled = false;
                ProgressPoller.Value = 0;
                TimerPoller.Enabled = true;

                if (_mem == null)
                {
                    LabelStatus.Text = "MemcachedClient returned a null object";
                }
                else
                {
                    ButtonAdd.Enabled = true;
                    ButtonGet.Enabled = true;
                    LabelStatus.Text = "Old Instantiation Success";
                    ProgressBarStatus.Value = 25;
                }

                #endregion
            }
            catch (Exception ex)
            {
                LabelStatus.Text = ex.Message;
            }
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                LabelStatus.Text = "Storing";

                // Stores the same as an Enyim client, just that the nodes are already set through the _config object
                var res = _mem.ExecuteStore(StoreMode.Set, TextKey.Text, TextValue.Text);
                if (res.Success)
                {
                    #region UI Stuff

                    TextGetKey.Text = TextKey.Text;
                    LabelStatus.Text = "Storing Success";
                    if (ProgressBarStatus.Value < 50)
                    {
                        ProgressBarStatus.Value = 50;
                    }

                    #endregion
                }
                else
                {
                    LabelStatus.Text = "Failed to store: " + res.Message;
                }
            }
            catch (Exception ex)
            {
                LabelStatus.Text = ex.Message;
            }
        }

        private void ButtonGet_Click(object sender, EventArgs e)
        {
            try
            {
                LabelStatus.Text = "Getting";

                // Gets the value the same way as a normal Enyim client from the dynamic nodes provided from the _config
                object val;
                if (!_mem.TryGet(TextGetKey.Text, out val))
                {

                #region UI Stuff

                    LabelStatus.Text = "Failed to get";
                }
                else
                {
                    LabelValue.Text = val as string;
                    LabelStatus.Text = "Getting Success";
                    if (ProgressBarStatus.Value < 75)
                    {
                        ProgressBarStatus.Value = 75;
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                LabelStatus.Text = ex.Message;
            }
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            if (_mem != null)
                _mem.Dispose();
            Application.Exit();
        }

        private void TimerPoller_Tick(object sender, EventArgs e)
        {
            LabelVersion.Text = String.Format("Config version is: {0}   ", _config.DiscoveryNode.ClusterVersion)
                                        + String.Format("Number of nodes: {0}", _config.DiscoveryNode.NodesInCluster);
            ProgressPoller.PerformStep();
            if (ProgressPoller.Value == 60)
            {
                ProgressPoller.Value = 0;
                ProgressBarStatus.Value = 100;                
                LabelStatus.Text = "Poller cycle completed."; 
            }
            
        }
    }
}
