namespace PinVol
{
    partial class UIWin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UIWin));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtGlobalDown = new System.Windows.Forms.TextBox();
            this.KeyMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.keyMenuNoKey = new System.Windows.Forms.ToolStripMenuItem();
            this.txtGlobalUp = new System.Windows.Forms.TextBox();
            this.txtLocalUp = new System.Windows.Forms.TextBox();
            this.txtLocalDown = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblGlobalVol = new System.Windows.Forms.Label();
            this.lblLocalVol = new System.Windows.Forms.Label();
            this.trkGlobalVol = new System.Windows.Forms.TrackBar();
            this.trkLocalVol = new System.Windows.Forms.TrackBar();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.lblSettingsBar = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblCurApp = new System.Windows.Forms.Label();
            this.btnHideSettings = new System.Windows.Forms.LinkLabel();
            this.btnShowSettings = new System.Windows.Forms.LinkLabel();
            this.ckExtIsLocal = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.btnSetUpOSD = new System.Windows.Forms.LinkLabel();
            this.ckOSDOnHotkeys = new System.Windows.Forms.CheckBox();
            this.appCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.ckOSDOnAppSwitch = new System.Windows.Forms.CheckBox();
            this.lblErrorAlert = new System.Windows.Forms.Label();
            this.btnViewErrors = new System.Windows.Forms.LinkLabel();
            this.lnkSelectDevices = new System.Windows.Forms.LinkLabel();
            this.label14 = new System.Windows.Forms.Label();
            this.txtMute = new System.Windows.Forms.TextBox();
            this.ckUnmuteOnVolChange = new System.Windows.Forms.CheckBox();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.label19 = new System.Windows.Forms.Label();
            this.ckEnableLocal2 = new System.Windows.Forms.CheckBox();
            this.label18 = new System.Windows.Forms.Label();
            this.txtLocal2Down = new System.Windows.Forms.TextBox();
            this.txtLocal2Up = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.txtNightMode = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.ckEnableJoystick = new System.Windows.Forms.CheckBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.picNightMode = new System.Windows.Forms.PictureBox();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.joystickRescanTimer = new System.Windows.Forms.Timer(this.components);
            this.tipVersion = new System.Windows.Forms.ToolTip(this.components);
            this.lblVersion = new System.Windows.Forms.Label();
            this.configToolCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.KeyMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkGlobalVol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkLocalVol)).BeginInit();
            this.settingsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNightMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(518, 78);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 132);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(390, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Global volume keys. These keys adjust the volume level for ALL tables/programs.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 280);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(534, 52);
            this.label3.TabIndex = 2;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 157);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Global volume up:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 181);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Global volume down:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 387);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Table volume down:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(26, 363);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Table volume up:";
            // 
            // label8
            // 
            this.label8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label8.Location = new System.Drawing.Point(12, 121);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(620, 1);
            this.label8.TabIndex = 7;
            // 
            // txtGlobalDown
            // 
            this.txtGlobalDown.BackColor = System.Drawing.SystemColors.Window;
            this.txtGlobalDown.ContextMenuStrip = this.KeyMenu;
            this.txtGlobalDown.Location = new System.Drawing.Point(138, 178);
            this.txtGlobalDown.Name = "txtGlobalDown";
            this.txtGlobalDown.ReadOnly = true;
            this.txtGlobalDown.Size = new System.Drawing.Size(138, 20);
            this.txtGlobalDown.TabIndex = 3;
            // 
            // KeyMenu
            // 
            this.KeyMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keyMenuNoKey});
            this.KeyMenu.Name = "Disable";
            this.KeyMenu.Size = new System.Drawing.Size(113, 26);
            this.KeyMenu.Text = "No Key";
            // 
            // keyMenuNoKey
            // 
            this.keyMenuNoKey.Name = "keyMenuNoKey";
            this.keyMenuNoKey.Size = new System.Drawing.Size(112, 22);
            this.keyMenuNoKey.Text = "No Key";
            this.keyMenuNoKey.Click += new System.EventHandler(this.keyMenuNoKey_Click);
            // 
            // txtGlobalUp
            // 
            this.txtGlobalUp.BackColor = System.Drawing.SystemColors.Window;
            this.txtGlobalUp.ContextMenuStrip = this.KeyMenu;
            this.txtGlobalUp.Location = new System.Drawing.Point(138, 154);
            this.txtGlobalUp.Name = "txtGlobalUp";
            this.txtGlobalUp.ReadOnly = true;
            this.txtGlobalUp.Size = new System.Drawing.Size(138, 20);
            this.txtGlobalUp.TabIndex = 2;
            // 
            // txtLocalUp
            // 
            this.txtLocalUp.BackColor = System.Drawing.SystemColors.Window;
            this.txtLocalUp.ContextMenuStrip = this.KeyMenu;
            this.txtLocalUp.Location = new System.Drawing.Point(138, 360);
            this.txtLocalUp.Name = "txtLocalUp";
            this.txtLocalUp.ReadOnly = true;
            this.txtLocalUp.Size = new System.Drawing.Size(138, 20);
            this.txtLocalUp.TabIndex = 6;
            // 
            // txtLocalDown
            // 
            this.txtLocalDown.BackColor = System.Drawing.SystemColors.Window;
            this.txtLocalDown.ContextMenuStrip = this.KeyMenu;
            this.txtLocalDown.Location = new System.Drawing.Point(138, 384);
            this.txtLocalDown.Name = "txtLocalDown";
            this.txtLocalDown.ReadOnly = true;
            this.txtLocalDown.Size = new System.Drawing.Size(138, 20);
            this.txtLocalDown.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(176, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 13);
            this.label10.TabIndex = 31;
            this.label10.Text = "Global volume:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(179, 101);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(74, 13);
            this.label11.TabIndex = 32;
            this.label11.Text = "Table volume:";
            // 
            // lblGlobalVol
            // 
            this.lblGlobalVol.AutoSize = true;
            this.lblGlobalVol.Location = new System.Drawing.Point(498, 47);
            this.lblGlobalVol.Name = "lblGlobalVol";
            this.lblGlobalVol.Size = new System.Drawing.Size(21, 13);
            this.lblGlobalVol.TabIndex = 33;
            this.lblGlobalVol.Text = "0%";
            // 
            // lblLocalVol
            // 
            this.lblLocalVol.AutoSize = true;
            this.lblLocalVol.Location = new System.Drawing.Point(498, 101);
            this.lblLocalVol.Name = "lblLocalVol";
            this.lblLocalVol.Size = new System.Drawing.Size(21, 13);
            this.lblLocalVol.TabIndex = 34;
            this.lblLocalVol.Text = "0%";
            // 
            // trkGlobalVol
            // 
            this.trkGlobalVol.Location = new System.Drawing.Point(267, 33);
            this.trkGlobalVol.Maximum = 100;
            this.trkGlobalVol.Name = "trkGlobalVol";
            this.trkGlobalVol.Size = new System.Drawing.Size(219, 45);
            this.trkGlobalVol.TabIndex = 0;
            this.trkGlobalVol.TickFrequency = 10;
            this.trkGlobalVol.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trkGlobalVol.Scroll += new System.EventHandler(this.trkGlobalVol_Scroll);
            // 
            // trkLocalVol
            // 
            this.trkLocalVol.Location = new System.Drawing.Point(267, 87);
            this.trkLocalVol.Maximum = 100;
            this.trkLocalVol.Name = "trkLocalVol";
            this.trkLocalVol.Size = new System.Drawing.Size(219, 45);
            this.trkLocalVol.TabIndex = 1;
            this.trkLocalVol.TickFrequency = 10;
            this.trkLocalVol.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trkLocalVol.Scroll += new System.EventHandler(this.trkLocalVol_Scroll);
            // 
            // updateTimer
            // 
            this.updateTimer.Interval = 33;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // lblSettingsBar
            // 
            this.lblSettingsBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSettingsBar.Location = new System.Drawing.Point(12, 5);
            this.lblSettingsBar.Name = "lblSettingsBar";
            this.lblSettingsBar.Size = new System.Drawing.Size(620, 1);
            this.lblSettingsBar.TabIndex = 37;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(140, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(113, 13);
            this.label9.TabIndex = 38;
            this.label9.Text = "Current table/program:";
            // 
            // lblCurApp
            // 
            this.lblCurApp.AutoSize = true;
            this.lblCurApp.Location = new System.Drawing.Point(264, 9);
            this.lblCurApp.Name = "lblCurApp";
            this.lblCurApp.Size = new System.Drawing.Size(39, 13);
            this.lblCurApp.TabIndex = 39;
            this.lblCurApp.Text = "(None)";
            // 
            // btnHideSettings
            // 
            this.btnHideSettings.AutoSize = true;
            this.btnHideSettings.Location = new System.Drawing.Point(566, 10);
            this.btnHideSettings.Name = "btnHideSettings";
            this.btnHideSettings.Size = new System.Drawing.Size(70, 13);
            this.btnHideSettings.TabIndex = 1;
            this.btnHideSettings.TabStop = true;
            this.btnHideSettings.Text = "Hide Settings";
            this.btnHideSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.btnHideSettings_LinkClicked);
            // 
            // btnShowSettings
            // 
            this.btnShowSettings.AutoSize = true;
            this.btnShowSettings.Location = new System.Drawing.Point(588, 116);
            this.btnShowSettings.Name = "btnShowSettings";
            this.btnShowSettings.Size = new System.Drawing.Size(45, 13);
            this.btnShowSettings.TabIndex = 3;
            this.btnShowSettings.TabStop = true;
            this.btnShowSettings.Text = "Settings";
            this.btnShowSettings.Visible = false;
            this.btnShowSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.btnShowSettings_LinkClicked);
            // 
            // ckExtIsLocal
            // 
            this.ckExtIsLocal.AutoSize = true;
            this.ckExtIsLocal.Location = new System.Drawing.Point(29, 544);
            this.ckExtIsLocal.Name = "ckExtIsLocal";
            this.ckExtIsLocal.Size = new System.Drawing.Size(287, 17);
            this.ckExtIsLocal.TabIndex = 12;
            this.ckExtIsLocal.Text = "Apply external volume changes to the current table only";
            this.ckExtIsLocal.UseVisualStyleBackColor = true;
            this.ckExtIsLocal.CheckedChanged += new System.EventHandler(this.ckExtIsLocal_CheckedChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(46, 564);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(442, 52);
            this.label12.TabIndex = 43;
            this.label12.Text = resources.GetString("label12.Text");
            // 
            // label13
            // 
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label13.Location = new System.Drawing.Point(11, 625);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(620, 1);
            this.label13.TabIndex = 44;
            // 
            // btnSetUpOSD
            // 
            this.btnSetUpOSD.AutoSize = true;
            this.btnSetUpOSD.Location = new System.Drawing.Point(285, 637);
            this.btnSetUpOSD.Name = "btnSetUpOSD";
            this.btnSetUpOSD.Size = new System.Drawing.Size(55, 13);
            this.btnSetUpOSD.TabIndex = 15;
            this.btnSetUpOSD.TabStop = true;
            this.btnSetUpOSD.Text = "Customize";
            this.btnSetUpOSD.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.btnSetUpOSD_LinkClicked);
            // 
            // ckOSDOnHotkeys
            // 
            this.ckOSDOnHotkeys.AutoSize = true;
            this.ckOSDOnHotkeys.Location = new System.Drawing.Point(13, 636);
            this.ckOSDOnHotkeys.Name = "ckOSDOnHotkeys";
            this.ckOSDOnHotkeys.Size = new System.Drawing.Size(254, 17);
            this.ckOSDOnHotkeys.TabIndex = 13;
            this.ckOSDOnHotkeys.Text = "Show volume overlay when hotkeys are pressed";
            this.ckOSDOnHotkeys.UseVisualStyleBackColor = true;
            this.ckOSDOnHotkeys.CheckedChanged += new System.EventHandler(this.ckOSDOnHotkeys_CheckedChanged);
            // 
            // appCheckTimer
            // 
            this.appCheckTimer.Enabled = true;
            this.appCheckTimer.Interval = 1000;
            this.appCheckTimer.Tick += new System.EventHandler(this.appCheckTimer_Tick);
            // 
            // ckOSDOnAppSwitch
            // 
            this.ckOSDOnAppSwitch.AutoSize = true;
            this.ckOSDOnAppSwitch.Location = new System.Drawing.Point(13, 659);
            this.ckOSDOnAppSwitch.Name = "ckOSDOnAppSwitch";
            this.ckOSDOnAppSwitch.Size = new System.Drawing.Size(265, 17);
            this.ckOSDOnAppSwitch.TabIndex = 14;
            this.ckOSDOnAppSwitch.Text = "Show when switching to a new table or application";
            this.ckOSDOnAppSwitch.UseVisualStyleBackColor = true;
            this.ckOSDOnAppSwitch.CheckedChanged += new System.EventHandler(this.ckOSDOnAppSwitch_CheckedChanged);
            // 
            // lblErrorAlert
            // 
            this.lblErrorAlert.AutoSize = true;
            this.lblErrorAlert.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblErrorAlert.ForeColor = System.Drawing.Color.Red;
            this.lblErrorAlert.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblErrorAlert.Location = new System.Drawing.Point(528, 9);
            this.lblErrorAlert.Name = "lblErrorAlert";
            this.lblErrorAlert.Size = new System.Drawing.Size(107, 16);
            this.lblErrorAlert.TabIndex = 49;
            this.lblErrorAlert.Text = "Errors logged!\r\n";
            this.lblErrorAlert.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblErrorAlert.Visible = false;
            // 
            // btnViewErrors
            // 
            this.btnViewErrors.AutoSize = true;
            this.btnViewErrors.Location = new System.Drawing.Point(575, 31);
            this.btnViewErrors.Name = "btnViewErrors";
            this.btnViewErrors.Size = new System.Drawing.Size(60, 13);
            this.btnViewErrors.TabIndex = 2;
            this.btnViewErrors.TabStop = true;
            this.btnViewErrors.Text = "View Errors";
            this.btnViewErrors.Visible = false;
            this.btnViewErrors.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.btnViewErrors_LinkClicked);
            // 
            // lnkSelectDevices
            // 
            this.lnkSelectDevices.AutoSize = true;
            this.lnkSelectDevices.Location = new System.Drawing.Point(10, 100);
            this.lnkSelectDevices.Name = "lnkSelectDevices";
            this.lnkSelectDevices.Size = new System.Drawing.Size(106, 13);
            this.lnkSelectDevices.TabIndex = 0;
            this.lnkSelectDevices.TabStop = true;
            this.lnkSelectDevices.Text = "Select audio devices";
            this.lnkSelectDevices.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSelectDevices_LinkClicked);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(63, 229);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(66, 13);
            this.label14.TabIndex = 56;
            this.label14.Text = "Mute toggle:";
            // 
            // txtMute
            // 
            this.txtMute.BackColor = System.Drawing.SystemColors.Window;
            this.txtMute.ContextMenuStrip = this.KeyMenu;
            this.txtMute.Location = new System.Drawing.Point(138, 226);
            this.txtMute.Name = "txtMute";
            this.txtMute.ReadOnly = true;
            this.txtMute.Size = new System.Drawing.Size(138, 20);
            this.txtMute.TabIndex = 5;
            // 
            // ckUnmuteOnVolChange
            // 
            this.ckUnmuteOnVolChange.AutoSize = true;
            this.ckUnmuteOnVolChange.Location = new System.Drawing.Point(138, 252);
            this.ckUnmuteOnVolChange.Name = "ckUnmuteOnVolChange";
            this.ckUnmuteOnVolChange.Size = new System.Drawing.Size(177, 17);
            this.ckUnmuteOnVolChange.TabIndex = 6;
            this.ckUnmuteOnVolChange.Text = "Un-mute on any volume change";
            this.ckUnmuteOnVolChange.UseVisualStyleBackColor = true;
            this.ckUnmuteOnVolChange.CheckedChanged += new System.EventHandler(this.ckUnmuteOnVolChange_CheckedChanged);
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this.label19);
            this.settingsPanel.Controls.Add(this.ckEnableLocal2);
            this.settingsPanel.Controls.Add(this.label18);
            this.settingsPanel.Controls.Add(this.txtLocal2Down);
            this.settingsPanel.Controls.Add(this.txtLocal2Up);
            this.settingsPanel.Controls.Add(this.label17);
            this.settingsPanel.Controls.Add(this.label16);
            this.settingsPanel.Controls.Add(this.txtNightMode);
            this.settingsPanel.Controls.Add(this.label15);
            this.settingsPanel.Controls.Add(this.ckEnableJoystick);
            this.settingsPanel.Controls.Add(this.label1);
            this.settingsPanel.Controls.Add(this.ckUnmuteOnVolChange);
            this.settingsPanel.Controls.Add(this.label2);
            this.settingsPanel.Controls.Add(this.label3);
            this.settingsPanel.Controls.Add(this.txtMute);
            this.settingsPanel.Controls.Add(this.label4);
            this.settingsPanel.Controls.Add(this.label14);
            this.settingsPanel.Controls.Add(this.label5);
            this.settingsPanel.Controls.Add(this.pictureBox3);
            this.settingsPanel.Controls.Add(this.label7);
            this.settingsPanel.Controls.Add(this.pictureBox2);
            this.settingsPanel.Controls.Add(this.label6);
            this.settingsPanel.Controls.Add(this.lnkSelectDevices);
            this.settingsPanel.Controls.Add(this.label8);
            this.settingsPanel.Controls.Add(this.txtGlobalDown);
            this.settingsPanel.Controls.Add(this.txtGlobalUp);
            this.settingsPanel.Controls.Add(this.ckOSDOnAppSwitch);
            this.settingsPanel.Controls.Add(this.txtLocalDown);
            this.settingsPanel.Controls.Add(this.ckOSDOnHotkeys);
            this.settingsPanel.Controls.Add(this.txtLocalUp);
            this.settingsPanel.Controls.Add(this.btnSetUpOSD);
            this.settingsPanel.Controls.Add(this.lblSettingsBar);
            this.settingsPanel.Controls.Add(this.label13);
            this.settingsPanel.Controls.Add(this.btnHideSettings);
            this.settingsPanel.Controls.Add(this.label12);
            this.settingsPanel.Controls.Add(this.ckExtIsLocal);
            this.settingsPanel.Location = new System.Drawing.Point(1, 138);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(634, 686);
            this.settingsPanel.TabIndex = 57;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(46, 439);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(420, 39);
            this.label19.TabIndex = 67;
            this.label19.Text = resources.GetString("label19.Text");
            // 
            // ckEnableLocal2
            // 
            this.ckEnableLocal2.AutoSize = true;
            this.ckEnableLocal2.Location = new System.Drawing.Point(29, 419);
            this.ckEnableLocal2.Name = "ckEnableLocal2";
            this.ckEnableLocal2.Size = new System.Drawing.Size(338, 17);
            this.ckEnableLocal2.TabIndex = 10;
            this.ckEnableLocal2.Text = "Enable independent per-table volume for secondary audio devices";
            this.ckEnableLocal2.UseVisualStyleBackColor = true;
            this.ckEnableLocal2.CheckedChanged += new System.EventHandler(this.ckEnableLocal2_CheckedChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(305, 343);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(98, 13);
            this.label18.TabIndex = 65;
            this.label18.Text = "Secondary devices";
            // 
            // txtLocal2Down
            // 
            this.txtLocal2Down.BackColor = System.Drawing.SystemColors.Window;
            this.txtLocal2Down.ContextMenuStrip = this.KeyMenu;
            this.txtLocal2Down.Location = new System.Drawing.Point(288, 384);
            this.txtLocal2Down.Name = "txtLocal2Down";
            this.txtLocal2Down.ReadOnly = true;
            this.txtLocal2Down.Size = new System.Drawing.Size(138, 20);
            this.txtLocal2Down.TabIndex = 9;
            // 
            // txtLocal2Up
            // 
            this.txtLocal2Up.BackColor = System.Drawing.SystemColors.Window;
            this.txtLocal2Up.ContextMenuStrip = this.KeyMenu;
            this.txtLocal2Up.Location = new System.Drawing.Point(288, 360);
            this.txtLocal2Up.Name = "txtLocal2Up";
            this.txtLocal2Up.ReadOnly = true;
            this.txtLocal2Up.Size = new System.Drawing.Size(138, 20);
            this.txtLocal2Up.TabIndex = 8;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(155, 343);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(105, 13);
            this.label17.TabIndex = 62;
            this.label17.Text = "Default audio device";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(32, 205);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(97, 13);
            this.label16.TabIndex = 61;
            this.label16.Text = "Night Mode toggle:";
            // 
            // txtNightMode
            // 
            this.txtNightMode.BackColor = System.Drawing.SystemColors.Window;
            this.txtNightMode.ContextMenuStrip = this.KeyMenu;
            this.txtNightMode.Location = new System.Drawing.Point(138, 202);
            this.txtNightMode.Name = "txtNightMode";
            this.txtNightMode.ReadOnly = true;
            this.txtNightMode.Size = new System.Drawing.Size(138, 20);
            this.txtNightMode.TabIndex = 4;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(46, 507);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(443, 26);
            this.label15.TabIndex = 59;
            this.label15.Text = "Joystick support adds a small amount of overhead that might slightly reduce overa" +
    "ll system \r\nperformance.  Un-check this box to eliminate the overhead if you don" +
    "\'t need joystick support.";
            // 
            // ckEnableJoystick
            // 
            this.ckEnableJoystick.AutoSize = true;
            this.ckEnableJoystick.Location = new System.Drawing.Point(29, 487);
            this.ckEnableJoystick.Name = "ckEnableJoystick";
            this.ckEnableJoystick.Size = new System.Drawing.Size(135, 17);
            this.ckEnableJoystick.TabIndex = 11;
            this.ckEnableJoystick.Text = "Enable joystick buttons";
            this.ckEnableJoystick.UseVisualStyleBackColor = true;
            this.ckEnableJoystick.CheckedChanged += new System.EventHandler(this.ckEnableJoystick_CheckedChanged);
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::PinVol.Properties.Resources.VolumeIcon;
            this.pictureBox3.Location = new System.Drawing.Point(552, 636);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(80, 25);
            this.pictureBox3.TabIndex = 55;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::PinVol.Properties.Resources.keyboardicon;
            this.pictureBox2.Location = new System.Drawing.Point(552, 132);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(80, 30);
            this.pictureBox2.TabIndex = 54;
            this.pictureBox2.TabStop = false;
            // 
            // picNightMode
            // 
            this.picNightMode.Image = global::PinVol.Properties.Resources.nightMode;
            this.picNightMode.InitialImage = ((System.Drawing.Image)(resources.GetObject("picNightMode.InitialImage")));
            this.picNightMode.Location = new System.Drawing.Point(525, 41);
            this.picNightMode.Name = "picNightMode";
            this.picNightMode.Size = new System.Drawing.Size(24, 24);
            this.picNightMode.TabIndex = 58;
            this.picNightMode.TabStop = false;
            this.picNightMode.Visible = false;
            // 
            // picLogo
            // 
            this.picLogo.Image = global::PinVol.Properties.Resources.PinVolSpeaker;
            this.picLogo.Location = new System.Drawing.Point(11, 9);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(120, 120);
            this.picLogo.TabIndex = 53;
            this.picLogo.TabStop = false;
            this.tipVersion.SetToolTip(this.picLogo, "VERSION NUMBER");
            this.picLogo.Click += new System.EventHandler(this.picLogo_Click);
            // 
            // joystickRescanTimer
            // 
            this.joystickRescanTimer.Interval = 1000;
            this.joystickRescanTimer.Tick += new System.EventHandler(this.joystickRescanTimer_Tick);
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(11, 116);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(42, 13);
            this.lblVersion.TabIndex = 4;
            this.lblVersion.Text = "Version";
            this.lblVersion.Visible = false;
            // 
            // configToolCheckTimer
            // 
            this.configToolCheckTimer.Enabled = true;
            this.configToolCheckTimer.Interval = 1000;
            this.configToolCheckTimer.Tick += new System.EventHandler(this.configToolCheckTimer_Tick);
            // 
            // UIWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 826);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.picNightMode);
            this.Controls.Add(this.settingsPanel);
            this.Controls.Add(this.btnViewErrors);
            this.Controls.Add(this.lblErrorAlert);
            this.Controls.Add(this.btnShowSettings);
            this.Controls.Add(this.lblCurApp);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.trkLocalVol);
            this.Controls.Add(this.trkGlobalVol);
            this.Controls.Add(this.lblLocalVol);
            this.Controls.Add(this.lblGlobalVol);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.picLogo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "UIWin";
            this.Text = "PinVol";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.UIWin_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trkGlobalVol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkLocalVol)).EndInit();
            this.settingsPanel.ResumeLayout(false);
            this.settingsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNightMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtGlobalDown;
        private System.Windows.Forms.TextBox txtGlobalUp;
        private System.Windows.Forms.TextBox txtLocalUp;
        private System.Windows.Forms.TextBox txtLocalDown;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblGlobalVol;
        private System.Windows.Forms.Label lblLocalVol;
        private System.Windows.Forms.TrackBar trkGlobalVol;
        private System.Windows.Forms.TrackBar trkLocalVol;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Label lblSettingsBar;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblCurApp;
        private System.Windows.Forms.LinkLabel btnHideSettings;
        private System.Windows.Forms.LinkLabel btnShowSettings;
        private System.Windows.Forms.CheckBox ckExtIsLocal;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.LinkLabel btnSetUpOSD;
        private System.Windows.Forms.CheckBox ckOSDOnHotkeys;
        private System.Windows.Forms.ContextMenuStrip KeyMenu;
        private System.Windows.Forms.ToolStripMenuItem keyMenuNoKey;
        private System.Windows.Forms.Timer appCheckTimer;
        private System.Windows.Forms.CheckBox ckOSDOnAppSwitch;
        private System.Windows.Forms.Label lblErrorAlert;
        private System.Windows.Forms.LinkLabel btnViewErrors;
        private System.Windows.Forms.LinkLabel lnkSelectDevices;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtMute;
        private System.Windows.Forms.CheckBox ckUnmuteOnVolChange;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.CheckBox ckEnableJoystick;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtNightMode;
        private System.Windows.Forms.PictureBox picNightMode;
        private System.Windows.Forms.Timer joystickRescanTimer;
        private System.Windows.Forms.ToolTip tipVersion;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtLocal2Down;
        private System.Windows.Forms.TextBox txtLocal2Up;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.CheckBox ckEnableLocal2;
        private System.Windows.Forms.Timer configToolCheckTimer;
    }
}

