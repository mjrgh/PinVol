namespace PinVol
{
    partial class GetKey
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
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.lblShift = new System.Windows.Forms.Label();
            this.lblControl = new System.Windows.Forms.Label();
            this.lblAlt = new System.Windows.Forms.Label();
            this.lblWindows = new System.Windows.Forms.Label();
            this.keyStateTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(217, 58);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Press a key...";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(205, 146);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 21);
            this.button1.TabIndex = 1;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblShift
            // 
            this.lblShift.AutoSize = true;
            this.lblShift.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblShift.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShift.ForeColor = System.Drawing.Color.DimGray;
            this.lblShift.Location = new System.Drawing.Point(104, 108);
            this.lblShift.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblShift.Name = "lblShift";
            this.lblShift.Padding = new System.Windows.Forms.Padding(18, 4, 18, 4);
            this.lblShift.Size = new System.Drawing.Size(66, 23);
            this.lblShift.TabIndex = 2;
            this.lblShift.Text = "Shift";
            // 
            // lblControl
            // 
            this.lblControl.AutoSize = true;
            this.lblControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblControl.ForeColor = System.Drawing.Color.DimGray;
            this.lblControl.Location = new System.Drawing.Point(178, 108);
            this.lblControl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblControl.Name = "lblControl";
            this.lblControl.Padding = new System.Windows.Forms.Padding(18, 4, 18, 4);
            this.lblControl.Size = new System.Drawing.Size(78, 23);
            this.lblControl.TabIndex = 3;
            this.lblControl.Text = "Control";
            // 
            // lblAlt
            // 
            this.lblAlt.AutoSize = true;
            this.lblAlt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblAlt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAlt.ForeColor = System.Drawing.Color.DimGray;
            this.lblAlt.Location = new System.Drawing.Point(264, 108);
            this.lblAlt.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAlt.Name = "lblAlt";
            this.lblAlt.Padding = new System.Windows.Forms.Padding(18, 4, 18, 4);
            this.lblAlt.Size = new System.Drawing.Size(57, 23);
            this.lblAlt.TabIndex = 4;
            this.lblAlt.Text = "Alt";
            // 
            // lblWindows
            // 
            this.lblWindows.AutoSize = true;
            this.lblWindows.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblWindows.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWindows.ForeColor = System.Drawing.Color.DimGray;
            this.lblWindows.Location = new System.Drawing.Point(329, 108);
            this.lblWindows.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWindows.Name = "lblWindows";
            this.lblWindows.Padding = new System.Windows.Forms.Padding(18, 4, 18, 4);
            this.lblWindows.Size = new System.Drawing.Size(89, 23);
            this.lblWindows.TabIndex = 5;
            this.lblWindows.Text = "Windows";
            // 
            // keyStateTimer
            // 
            this.keyStateTimer.Enabled = true;
            this.keyStateTimer.Tick += new System.EventHandler(this.keyStateTimer_Tick);
            // 
            // GetKey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Navy;
            this.ClientSize = new System.Drawing.Size(522, 177);
            this.ControlBox = false;
            this.Controls.Add(this.lblWindows);
            this.Controls.Add(this.lblAlt);
            this.Controls.Add(this.lblControl);
            this.Controls.Add(this.lblShift);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Wingdings", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GetKey";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GetKey";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GetKey_FormClosed);
            this.Load += new System.EventHandler(this.GetKey_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblShift;
        private System.Windows.Forms.Label lblControl;
        private System.Windows.Forms.Label lblAlt;
        private System.Windows.Forms.Label lblWindows;
        private System.Windows.Forms.Timer keyStateTimer;
    }
}