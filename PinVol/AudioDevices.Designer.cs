namespace PinVol
{
    partial class AudioOutputs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioOutputs));
            this.label2 = new System.Windows.Forms.Label();
            this.ckActive1 = new System.Windows.Forms.CheckBox();
            this.trkVolume1 = new System.Windows.Forms.TrackBar();
            this.lblVolume1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.subpanel1 = new System.Windows.Forms.Panel();
            this.ckUseLocal1 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.trkVolume1)).BeginInit();
            this.panel1.SuspendLayout();
            this.subpanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(457, 65);
            this.label2.TabIndex = 1;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // ckActive1
            // 
            this.ckActive1.AutoSize = true;
            this.ckActive1.Location = new System.Drawing.Point(10, 7);
            this.ckActive1.Name = "ckActive1";
            this.ckActive1.Size = new System.Drawing.Size(127, 17);
            this.ckActive1.TabIndex = 2;
            this.ckActive1.Text = "Default Audio Device";
            this.ckActive1.UseVisualStyleBackColor = true;
            this.ckActive1.CheckedChanged += new System.EventHandler(this.ckActive1_CheckedChanged);
            // 
            // trkVolume1
            // 
            this.trkVolume1.Location = new System.Drawing.Point(25, 59);
            this.trkVolume1.Maximum = 200;
            this.trkVolume1.Name = "trkVolume1";
            this.trkVolume1.Size = new System.Drawing.Size(229, 45);
            this.trkVolume1.TabIndex = 3;
            this.trkVolume1.TickFrequency = 10;
            // 
            // lblVolume1
            // 
            this.lblVolume1.AutoSize = true;
            this.lblVolume1.Location = new System.Drawing.Point(260, 62);
            this.lblVolume1.Name = "lblVolume1";
            this.lblVolume1.Size = new System.Drawing.Size(33, 13);
            this.lblVolume1.TabIndex = 4;
            this.lblVolume1.Text = "100%";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.subpanel1);
            this.panel1.Location = new System.Drawing.Point(-1, 85);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(514, 229);
            this.panel1.TabIndex = 6;
            this.panel1.Resize += new System.EventHandler(this.panel1_Resize);
            // 
            // subpanel1
            // 
            this.subpanel1.Controls.Add(this.ckUseLocal1);
            this.subpanel1.Controls.Add(this.trkVolume1);
            this.subpanel1.Controls.Add(this.ckActive1);
            this.subpanel1.Controls.Add(this.lblVolume1);
            this.subpanel1.Controls.Add(this.label1);
            this.subpanel1.Location = new System.Drawing.Point(0, 0);
            this.subpanel1.Name = "subpanel1";
            this.subpanel1.Size = new System.Drawing.Size(514, 110);
            this.subpanel1.TabIndex = 8;
            // 
            // ckUseLocal1
            // 
            this.ckUseLocal1.AutoSize = true;
            this.ckUseLocal1.Location = new System.Drawing.Point(32, 30);
            this.ckUseLocal1.Name = "ckUseLocal1";
            this.ckUseLocal1.Size = new System.Drawing.Size(242, 17);
            this.ckUseLocal1.TabIndex = 7;
            this.ckUseLocal1.Text = "Apply the per-table volume level to this device";
            this.ckUseLocal1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(401, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "This device is the reference point for the relative volume levels of the other de" +
    "vices.";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(425, 324);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // AudioOutputs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 359);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AudioOutputs";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Audio Outputs";
            this.Load += new System.EventHandler(this.AudioOutputs_Load);
            this.Resize += new System.EventHandler(this.AudioOutputs_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.trkVolume1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.subpanel1.ResumeLayout(false);
            this.subpanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox ckActive1;
        private System.Windows.Forms.TrackBar trkVolume1;
        private System.Windows.Forms.Label lblVolume1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox ckUseLocal1;
        private System.Windows.Forms.Panel subpanel1;
    }
}