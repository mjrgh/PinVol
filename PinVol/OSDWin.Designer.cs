namespace PinVol
{
    partial class OSDWin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OSDWin));
            this.btnCW = new System.Windows.Forms.Button();
            this.btnCCW = new System.Windows.Forms.Button();
            this.btnDone = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCW
            // 
            this.btnCW.BackColor = System.Drawing.Color.DimGray;
            this.btnCW.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnCW.FlatAppearance.BorderSize = 0;
            this.btnCW.Image = global::PinVol.Properties.Resources.RotateCW;
            this.btnCW.Location = new System.Drawing.Point(112, 702);
            this.btnCW.Name = "btnCW";
            this.btnCW.Size = new System.Drawing.Size(68, 68);
            this.btnCW.TabIndex = 0;
            this.btnCW.UseVisualStyleBackColor = false;
            this.btnCW.Visible = false;
            this.btnCW.Click += new System.EventHandler(this.btnCW_Click);
            // 
            // btnCCW
            // 
            this.btnCCW.BackColor = System.Drawing.Color.DimGray;
            this.btnCCW.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnCCW.FlatAppearance.BorderSize = 0;
            this.btnCCW.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnCCW.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnCCW.Image = global::PinVol.Properties.Resources.RotateCCW;
            this.btnCCW.Location = new System.Drawing.Point(0, 702);
            this.btnCCW.Name = "btnCCW";
            this.btnCCW.Size = new System.Drawing.Size(68, 68);
            this.btnCCW.TabIndex = 1;
            this.btnCCW.UseVisualStyleBackColor = false;
            this.btnCCW.Visible = false;
            this.btnCCW.Click += new System.EventHandler(this.btnCCW_Click);
            // 
            // btnDone
            // 
            this.btnDone.Location = new System.Drawing.Point(50, 374);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(75, 23);
            this.btnDone.TabIndex = 2;
            this.btnDone.Text = "Done";
            this.btnDone.UseVisualStyleBackColor = true;
            this.btnDone.Visible = false;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // OSDWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(222)))), ((int)(((byte)(222)))));
            this.ClientSize = new System.Drawing.Size(181, 770);
            this.ControlBox = false;
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.btnCCW);
            this.Controls.Add(this.btnCW);
            this.Enabled = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OSDWin";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Volume";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(222)))), ((int)(((byte)(222)))));
            this.Load += new System.EventHandler(this.OSDWin_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.VolumeOverlay_Paint);
            this.Move += new System.EventHandler(this.OSDWin_Move);
            this.Resize += new System.EventHandler(this.OSDWin_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCW;
        private System.Windows.Forms.Button btnCCW;
        private System.Windows.Forms.Button btnDone;

    }
}