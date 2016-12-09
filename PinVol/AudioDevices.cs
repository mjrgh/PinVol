using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PinVol
{
    public partial class AudioOutputs : Form
    {
        public AudioOutputs(UIWin.AudioDevice[] devices, UIWin.AudioDevice defaultDevice)
        {
            this.devices = devices;
            this.defaultDevice = defaultDevice;
            InitializeComponent();
        }

        UIWin.AudioDevice[] devices;
        UIWin.AudioDevice defaultDevice;

        private void AudioOutputs_Load(object sender, EventArgs e)
        {
            // note the initial layout, so we can move things around on resize
            panel1BottomMargin = ClientSize.Height - panel1.Bottom;
            closeButtonMargin.Width = ClientSize.Width - btnClose.Right;
            closeButtonMargin.Height = ClientSize.Height - btnClose.Bottom;

            // build the audio device list
            BuildDeviceList();
        }

        void BuildDeviceList()
        {
            // run through the list of audio devices, adding an entry for
            // each one to the panel
            Panel lastsubpanel = subpanel1;
            int rownum = 1;
            foreach (UIWin.AudioDevice dev in devices)
            {
                // If this is the default device, set it up in the first slot.
                // Otherwise, create a new slot for it.
                CheckBox ckActive, ckUseLocal;
                TrackBar trkVol;
                Label lblVol;
                Panel subpanel;
                if (dev == defaultDevice)
                {
                    // It's the default device.  The first row serves as the
                    // template for additional devices, but it's also the actual
                    // row for the default device, so set up its controls to
                    // reflect the default device data.
                    ckActive = ckActive1;
                    ckUseLocal = ckUseLocal1;
                    trkVol = trkVolume1;
                    lblVol = lblVolume1;
                    subpanel = subpanel1;

                    // The device is always enabled; the Enabled checkbox is
                    // labeled with the device name.
                    ckActive.Checked = true;
                    ckActive.Text = dev.name + " (Default audio output)";

                    // The default device relative volume is always fixed at 100%,
                    // since it's the reference point for the relative levels of the
                    // other devices.
                    trkVol.Value = 100;
                    trkVol.Visible = false;
                    lblVol.Visible = false;
                }
                else
                {
                    // It's not the default device, so set up a new row for it,
                    // with new controls.
                    ++rownum;

                    // set up the subpanel
                    subpanel = new Panel();
                    subpanel.Name = "subpanel" + rownum;

                    // Set up the Enabled checkbox.  This is labeled with the
                    // device name.
                    ckActive = new CheckBox();
                    ckActive.Name = "ckActive1";
                    ckActive.Text = dev.name;
                    ckActive.AutoSize = true;
                    ckActive.Checked = dev.isActive;

                    // set up the Global Only checkbox
                    ckUseLocal = new CheckBox();
                    ckUseLocal.Text = ckUseLocal1.Text;
                    ckUseLocal.Size = ckUseLocal1.Size;
                    ckUseLocal.Checked = dev.useLocal;

                    // set up the new volume bar
                    trkVol = new TrackBar();
                    trkVol.Minimum = 0;
                    trkVol.Maximum = 200;
                    trkVol.Value = (int)(dev.relLevel * 100.0f);
                    trkVol.TickFrequency = 10;
                    trkVol.Size = trkVolume1.Size;

                    // set up the new volume label
                    lblVol = new Label();
                    lblVol.Text = trkVol.Value + "%";

                    // set up a separator bar
                    Label bar = new Label();
                    bar.Name = "sepBar";
                    bar.Height = 1;
                    bar.Width = subpanel1.Width;
                    bar.BorderStyle = BorderStyle.FixedSingle;
                    bar.Location = new Point(0, 0);

                    // Wire up the events.  Note that we need to create closures to
                    // capture the current loop variables, since otherwise we'd bind
                    // to the live variables, which will change as we iterate and thus
                    // won't be right by the time the event callbacks are invoked.                    

                    // active checkbox - update the device status
                    ckActive.CheckedChanged += 
                        ((Func<UIWin.AudioDevice, CheckBox, TrackBar, CheckBox, Panel, EventHandler>)
                        ((UIWin.AudioDevice idev, CheckBox ck, TrackBar tb, CheckBox cklcl, Panel sp) =>
                    {
                        return (object sender, EventArgs e) =>
                        {
                            // update the device record
                            idev.isActive = ck.Checked;

                            // adjust heights
                            AdjustPanelHeights();
                        };
                    }))(dev, ckActive, trkVol, ckUseLocal, subpanel);

                    // trackbar - update the volume display and device volume level
                    trkVol.Scroll += ((Func<UIWin.AudioDevice, TrackBar, Label, EventHandler>)((UIWin.AudioDevice idev, TrackBar t, Label l) =>
                    {
                        return (object sender, EventArgs e) =>
                        {
                            l.Text = t.Value + "%";
                            idev.relLevel = t.Value / 100.0f;
                        };
                    }))(dev, trkVol, lblVol);

                    // add the controls to the panel
                    panel1.Controls.Add(subpanel);
                    subpanel.Controls.Add(ckActive);
                    subpanel.Controls.Add(ckUseLocal);
                    subpanel.Controls.Add(trkVol);
                    subpanel.Controls.Add(lblVol);
                    subpanel.Controls.Add(bar);

                    // Position the controls.  Position the subpanel under the last subpanel,
                    // and position all of the controls within the subpanel to match the
                    // positions of the first row's controls relative to its subpanel.
                    subpanel.Location = new Point(subpanel1.Location.X, subpanel1.Bottom);
                    subpanel.Size = subpanel1.Size;

                    ckActive.Location = ckActive1.Location;
                    trkVol.Location = trkVolume1.Location;
                    lblVol.Location = lblVolume1.Location;
                    ckUseLocal.Location = ckUseLocal1.Location;
                }

                // set the Global Only flag
                ckUseLocal.Checked = dev.useLocal;

                // global only checkbox - update the device status
                ckUseLocal.CheckedChanged += ((Func<UIWin.AudioDevice, CheckBox, EventHandler>)((UIWin.AudioDevice idev, CheckBox ck) =>
                {
                    return (object sender, EventArgs e) =>
                    {
                        // update the device record
                        idev.useLocal = ck.Checked;
                    };
                }))(dev, ckUseLocal);
            }

            // adjust the heights to reflect current enabled/disabled status
            AdjustPanelHeights();

            // refigure scrolling in the panel, since we might have changed
            // the height of its contents
            ResetScrolling();
        }

        // Adjust subpanel heights for enabled/disabled status
        void AdjustPanelHeights()
        {
            int y = 0;
            foreach (Control c in panel1.Controls)
            {
                if (c.Name.StartsWith("subpanel"))
                {
                    c.Location = new Point(0, y);
                    CheckBox ck = c.Controls["ckActive1"] as CheckBox;
                    int ht = ck.Checked ? subpanel1.Height : ck.Bottom + ck.Top;
                    c.Height = ht;
                    y += ht;
                }
            }
        }

        // Refigure scrolling in the panel.  It seems to be necessary to do this
        // after changing the panel size to get the internal range calculations
        // to update properly.
        void ResetScrolling()
        {
            panel1.AutoScroll = true;
        }

        int panel1BottomMargin;     // distance between bottom of panel and bottom of window
        Size closeButtonMargin;     // bottom right margin of Close button
        private void AudioOutputs_Resize(object sender, EventArgs e)
        {
            // reposition controls
            panel1.Width = ClientSize.Width+2;
            panel1.Height = ClientSize.Height - panel1.Top - panel1BottomMargin;
            btnClose.Left = ClientSize.Width - closeButtonMargin.Width - btnClose.Width;
            btnClose.Top = ClientSize.Height - closeButtonMargin.Height - btnClose.Height;
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            ResetScrolling();
            foreach (Control c in panel1.Controls)
            {
                if (c.Name.StartsWith("subpanel"))
                {
                    int w = ClientSize.Width;
                    c.Width = w;
                    Control bar = c.Controls["sepBar"];
                    if (bar != null)
                        bar.Width = w;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ckActive1_CheckedChanged(object sender, EventArgs e)
        {
            if (!ckActive1.Checked)
            {
                ckActive1.Checked = true;
                MessageBox.Show("The default audio device can't be disabled, since it's used "
                    + "to determine the reference volume level for the other outputs. If you "
                    + "don't want PinVol to control this device, you can change the default "
                    + "device using the Windows \"Sound\" control panel. Select a device "
                    + "there that you do want PinVol to control.");
            }
        }

    }
}
