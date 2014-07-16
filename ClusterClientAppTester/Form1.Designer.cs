namespace ClusterClientAppTester
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ButtonOlder = new System.Windows.Forms.Button();
            this.TextOlder = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TextPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ButtonInstantiate = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TextValue = new System.Windows.Forms.TextBox();
            this.TextKey = new System.Windows.Forms.TextBox();
            this.ButtonAdd = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.LabelValue = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.TextGetKey = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ButtonGet = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.LabelStatus = new System.Windows.Forms.Label();
            this.ProgressBarStatus = new System.Windows.Forms.ProgressBar();
            this.ButtonExit = new System.Windows.Forms.Button();
            this.ProgressPoller = new System.Windows.Forms.ProgressBar();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.LabelVersion = new System.Windows.Forms.Label();
            this.TimerPoller = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ButtonOlder);
            this.groupBox1.Controls.Add(this.TextOlder);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.TextPort);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.ButtonInstantiate);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(317, 125);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection Tests";
            // 
            // ButtonOlder
            // 
            this.ButtonOlder.Location = new System.Drawing.Point(159, 85);
            this.ButtonOlder.Name = "ButtonOlder";
            this.ButtonOlder.Size = new System.Drawing.Size(133, 23);
            this.ButtonOlder.TabIndex = 7;
            this.ButtonOlder.Text = "Instantiate From Params";
            this.ButtonOlder.UseVisualStyleBackColor = true;
            this.ButtonOlder.Click += new System.EventHandler(this.ButtonOlder_Click);
            // 
            // TextOlder
            // 
            this.TextOlder.Location = new System.Drawing.Point(6, 48);
            this.TextOlder.Name = "TextOlder";
            this.TextOlder.Size = new System.Drawing.Size(172, 20);
            this.TextOlder.TabIndex = 6;
            this.TextOlder.Text = "versiontesting.4mpstz.cfg.use1.cache.amazonaws.com";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(199, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Endpoint Port:";
            // 
            // TextPort
            // 
            this.TextPort.Location = new System.Drawing.Point(202, 48);
            this.TextPort.Name = "TextPort";
            this.TextPort.Size = new System.Drawing.Size(41, 20);
            this.TextPort.TabIndex = 3;
            this.TextPort.Text = "11211";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Endpoint Name:";
            // 
            // ButtonInstantiate
            // 
            this.ButtonInstantiate.Location = new System.Drawing.Point(6, 85);
            this.ButtonInstantiate.Name = "ButtonInstantiate";
            this.ButtonInstantiate.Size = new System.Drawing.Size(147, 23);
            this.ButtonInstantiate.TabIndex = 0;
            this.ButtonInstantiate.Text = "Instantiate From App.Config";
            this.ButtonInstantiate.UseVisualStyleBackColor = true;
            this.ButtonInstantiate.Click += new System.EventHandler(this.ButtonInstantiate_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.TextValue);
            this.groupBox2.Controls.Add(this.TextKey);
            this.groupBox2.Controls.Add(this.ButtonAdd);
            this.groupBox2.Location = new System.Drawing.Point(335, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(235, 125);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Initial Add";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(109, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Value:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Key:";
            // 
            // TextValue
            // 
            this.TextValue.Location = new System.Drawing.Point(112, 48);
            this.TextValue.Name = "TextValue";
            this.TextValue.Size = new System.Drawing.Size(100, 20);
            this.TextValue.TabIndex = 2;
            this.TextValue.Text = "Hello World";
            // 
            // TextKey
            // 
            this.TextKey.Location = new System.Drawing.Point(6, 48);
            this.TextKey.Name = "TextKey";
            this.TextKey.Size = new System.Drawing.Size(90, 20);
            this.TextKey.TabIndex = 1;
            this.TextKey.Text = "Test";
            // 
            // ButtonAdd
            // 
            this.ButtonAdd.Enabled = false;
            this.ButtonAdd.Location = new System.Drawing.Point(6, 96);
            this.ButtonAdd.Name = "ButtonAdd";
            this.ButtonAdd.Size = new System.Drawing.Size(89, 23);
            this.ButtonAdd.TabIndex = 0;
            this.ButtonAdd.Text = "Add to Cache";
            this.ButtonAdd.UseVisualStyleBackColor = true;
            this.ButtonAdd.Click += new System.EventHandler(this.ButtonAdd_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.LabelValue);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.TextGetKey);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.ButtonGet);
            this.groupBox3.Location = new System.Drawing.Point(12, 144);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(317, 109);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Initial Get";
            // 
            // LabelValue
            // 
            this.LabelValue.AutoSize = true;
            this.LabelValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelValue.Location = new System.Drawing.Point(139, 46);
            this.LabelValue.Name = "LabelValue";
            this.LabelValue.Size = new System.Drawing.Size(0, 25);
            this.LabelValue.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(141, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Value:";
            // 
            // TextGetKey
            // 
            this.TextGetKey.Location = new System.Drawing.Point(9, 37);
            this.TextGetKey.Name = "TextGetKey";
            this.TextGetKey.ReadOnly = true;
            this.TextGetKey.Size = new System.Drawing.Size(100, 20);
            this.TextGetKey.TabIndex = 2;
            this.TextGetKey.Text = "Test";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Key:";
            // 
            // ButtonGet
            // 
            this.ButtonGet.Enabled = false;
            this.ButtonGet.Location = new System.Drawing.Point(6, 80);
            this.ButtonGet.Name = "ButtonGet";
            this.ButtonGet.Size = new System.Drawing.Size(75, 23);
            this.ButtonGet.TabIndex = 0;
            this.ButtonGet.Text = "Get Value";
            this.ButtonGet.UseVisualStyleBackColor = true;
            this.ButtonGet.Click += new System.EventHandler(this.ButtonGet_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 275);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "Status:";
            // 
            // LabelStatus
            // 
            this.LabelStatus.AutoSize = true;
            this.LabelStatus.Location = new System.Drawing.Point(58, 275);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(93, 13);
            this.LabelStatus.TabIndex = 4;
            this.LabelStatus.Text = "Waiting on action.";
            // 
            // ProgressBarStatus
            // 
            this.ProgressBarStatus.Location = new System.Drawing.Point(11, 300);
            this.ProgressBarStatus.Name = "ProgressBarStatus";
            this.ProgressBarStatus.Size = new System.Drawing.Size(140, 23);
            this.ProgressBarStatus.TabIndex = 5;
            // 
            // ButtonExit
            // 
            this.ButtonExit.Location = new System.Drawing.Point(495, 300);
            this.ButtonExit.Name = "ButtonExit";
            this.ButtonExit.Size = new System.Drawing.Size(75, 23);
            this.ButtonExit.TabIndex = 6;
            this.ButtonExit.Text = "Exit";
            this.ButtonExit.UseVisualStyleBackColor = true;
            this.ButtonExit.Click += new System.EventHandler(this.ButtonExit_Click);
            // 
            // ProgressPoller
            // 
            this.ProgressPoller.Location = new System.Drawing.Point(40, 48);
            this.ProgressPoller.Maximum = 60;
            this.ProgressPoller.Name = "ProgressPoller";
            this.ProgressPoller.Size = new System.Drawing.Size(157, 23);
            this.ProgressPoller.Step = 1;
            this.ProgressPoller.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(40, 29);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(145, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Wait 1 minute to check poller";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.LabelVersion);
            this.groupBox4.Controls.Add(this.ProgressPoller);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Location = new System.Drawing.Point(336, 144);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(234, 109);
            this.groupBox4.TabIndex = 9;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Poller Test";
            // 
            // LabelVersion
            // 
            this.LabelVersion.AutoSize = true;
            this.LabelVersion.Location = new System.Drawing.Point(6, 80);
            this.LabelVersion.Name = "LabelVersion";
            this.LabelVersion.Size = new System.Drawing.Size(0, 13);
            this.LabelVersion.TabIndex = 9;
            // 
            // TimerPoller
            // 
            this.TimerPoller.Interval = 1000;
            this.TimerPoller.Tick += new System.EventHandler(this.TimerPoller_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 335);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.ButtonExit);
            this.Controls.Add(this.ProgressBarStatus);
            this.Controls.Add(this.LabelStatus);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "Cluster Client Tests";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TextPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ButtonInstantiate;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button ButtonAdd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TextValue;
        private System.Windows.Forms.TextBox TextKey;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label LabelValue;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TextGetKey;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button ButtonGet;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label LabelStatus;
        private System.Windows.Forms.ProgressBar ProgressBarStatus;
        private System.Windows.Forms.TextBox TextOlder;
        private System.Windows.Forms.Button ButtonOlder;
        private System.Windows.Forms.Button ButtonExit;
        private System.Windows.Forms.ProgressBar ProgressPoller;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Timer TimerPoller;
        private System.Windows.Forms.Label LabelVersion;
    }
}

