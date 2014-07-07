using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Enyim.Caching;
using NetClusterClient;

namespace ClusterClientAppTester
{
    public partial class Form1 : Form
    {
        private MemcachedClient mem;
        private AutoClientConfig config;

        public Form1()
        {
            InitializeComponent();
        }

        public void ErrorAlert(string error)
        {
            MessageBox.Show(error);
        }

        private void ButtonInstantiate_Click(object sender, EventArgs e)
        {
            try
            {
                this.LabelStatus.Text = "Instantiating";

                // Instantiates config from app.config in the clusterclient section
                this.config = new AutoClientConfig();

                mem = new MemcachedClient(config);

                #region UI Stuff

                this.TimerPoller.Enabled = false;
                this.ProgressPoller.Value = 0;
                this.TimerPoller.Enabled = true;

                if (mem == null)
                {
                    this.LabelStatus.Text = "MemcachedClient returned a null object";
                }
                else
                {
                    this.ButtonAdd.Enabled = true;
                    this.ButtonGet.Enabled = false;
                    this.LabelStatus.Text = "Instantiation Success";
                    this.ProgressBarStatus.Value = 25;
                }

                #endregion
            }
            catch (Exception ex)
            {
                this.LabelStatus.Text = ex.Message + " " + ex.InnerException.Message;
            }
        }

        private void ButtonOlder_Click(object sender, EventArgs e)
        {
            try
            {
                this.LabelStatus.Text = "Instantiating";

                // Instantiates client with default settings and uses the hostname and port provided
                this.config = new AutoClientConfig(this.TextOlder.Text, Convert.ToInt32(this.TextPort.Text));

                mem = new MemcachedClient(config);

                #region UI Stuff

                this.TimerPoller.Enabled = false;
                this.ProgressPoller.Value = 0;
                this.TimerPoller.Enabled = true;

                if (mem == null)
                {
                    this.LabelStatus.Text = "MemcachedClient returned a null object";
                }
                else
                {
                    this.ButtonAdd.Enabled = true;
                    this.ButtonGet.Enabled = false;
                    this.LabelStatus.Text = "Old Instantiation Success";
                    this.ProgressBarStatus.Value = 25;
                }

                #endregion
            }
            catch (Exception ex)
            {
                this.LabelStatus.Text = ex.Message;
            }
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                this.LabelStatus.Text = "Storing";

                // Stores the same as an Enyim client, just that the nodes are already set through the config object
                if (mem.Store(Enyim.Caching.Memcached.StoreMode.Set, this.TextKey.Text, this.TextValue.Text))
                {
                    #region UI Stuff

                    this.TextGetKey.Text = this.TextKey.Text;
                    this.ButtonGet.Enabled = true;
                    this.LabelStatus.Text = "Storing Success";
                    if (this.ProgressBarStatus.Value < 50)
                    {
                        this.ProgressBarStatus.Value = 50;
                    }

                    #endregion
                }
                else
                {
                    this.LabelStatus.Text = "Failed to store";
                }
            }
            catch (Exception ex)
            {
                this.LabelStatus.Text = ex.Message;
            }
        }

        private void ButtonGet_Click(object sender, EventArgs e)
        {
            try
            {
                this.LabelStatus.Text = "Getting";

                // Gets the value the same way as a normal Enyim client from the dynamic nodes provided from the config
                var value = mem.Get(TextGetKey.Text) as string;

                #region UI Stuff

                if (value == null)
                {
                    this.LabelStatus.Text = "Failed to get";
                }
                else
                {
                    this.LabelValue.Text = value;
                    this.LabelStatus.Text = "Getting Success";
                    if (this.ProgressBarStatus.Value < 75)
                    {
                        this.ProgressBarStatus.Value = 75;
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                this.LabelStatus.Text = ex.Message;
            }
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            if (mem != null)
                this.mem.Dispose();
            Application.Exit();
        }

        private void TimerPoller_Tick(object sender, EventArgs e)
        {
            this.LabelVersion.Text = String.Format("Config version is: {0}   ", this.config.DiscoveryNode.ClusterVersion)
                                        + String.Format("Number of nodes: {0}", this.config.DiscoveryNode.NodesInCluster);
            this.ProgressPoller.PerformStep();
            if (this.ProgressPoller.Value == 60)
            {
                this.ProgressPoller.Value = 0;
                this.ProgressBarStatus.Value = 100;                
                this.LabelStatus.Text = "Poller cycle completed."; 
            }
            
        }
    }
}
