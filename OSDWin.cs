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
    public partial class OSDWin : Form
    {
        public enum OSDType
        {
            None,
            Local,      // local table setting for default audio device
            Local2,     // local table setting for secondary audio device
            Global,      // global volume setting
            SSFBG,         // SSF Back Glass volume setting
            SSFRS,         // SSF Rear Sides volume setting
            SSFFS,         // SSF Front Sides volume setting
        };

        public OSDWin(UIWin mainwin)
        {
            this.mainwin = mainwin;
            InitializeComponent();

            // set up drawing objects for the label text
            font = new Font(SystemFonts.CaptionFont.FontFamily, 24.0f);
            titleFmt = new StringFormat(StringFormatFlags.NoWrap);
            titleFmt.Alignment = StringAlignment.Center;
            titleFmt.LineAlignment = StringAlignment.Far;

            // set the night mode icon transparency
            nightMode.MakeTransparent(nightMode.GetPixel(0, 0));
        }

        // the main window (source of the current volume level)
        UIWin mainwin;

        // show without activation
        protected override bool ShowWithoutActivation { 
            get { return true; } 
        }

        // setup mode
        bool setupMode = false;
        Color origTransparencyKey;
        public bool InSetup() { return setupMode; }
        public void Setup(bool f)
        {
            // do nothing if already in the desired mode
            if (setupMode == f)
                return;

            // set the new mode
            if (f)
            {
                // turn off transparency
                origTransparencyKey = TransparencyKey;
                TransparencyKey = Color.Empty;

                // turn the borders on for sizing and positioning
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;

                // move to account for the border controls
                Point zero = PointToScreen(new Point(0, 0));
                Location = new Point(Location.X - (zero.X - Location.X), Location.Y - (zero.Y - Location.Y));

                // show the setup controls
                btnDone.Visible = true;
                btnCCW.Visible = true;
                btnCW.Visible = true;

                // show the window
                Opacity = 1.0;
                Visible = true;
                Enabled = true;

                // switch to local volume display, since that uses the larger text caption
                mainwin.osdType = OSDType.Local;

                // flag that we're now in setup mode
                setupMode = true;
            }
            else
            {
                // we're no longer in setup mode
                setupMode = false;

                // restore transparency
                TransparencyKey = origTransparencyKey;

                // hide the setup controls
                btnDone.Visible = false;
                btnCCW.Visible = false;
                btnCW.Visible = false;

                // disable the whole window so it won't accidentally grab focus
                Enabled = false;

                // move to account for the the disappearing border controls
                Point zero = PointToScreen(new Point(0, 0));
                Location = new Point(Location.X + (zero.X - Location.X), Location.Y + (zero.Y - Location.Y));

                // turn off the border
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

                // fade the display
                mainwin.BeginFadeOSD();
            }
        }

        private void VolumeOverlay_Paint(object sender, PaintEventArgs e)
        {
            // get the graphics context
            Graphics gr = e.Graphics;

            // rotate the coordinate system
            int r = mainwin.cfg.OSDRotation;
            switch (r)
            {
                case 0: gr.TranslateTransform(0, 0); break;
                case 90: gr.TranslateTransform(0, ClientSize.Height); break;
                case 180: gr.TranslateTransform(ClientSize.Width, ClientSize.Height); break;
                case 270: gr.TranslateTransform(ClientSize.Width, 0); break;
            }
            gr.RotateTransform(-r);

            // note in the title if muted
            String muted = mainwin.globalMute ? " (Muted)" : "";

            // draw the selected volume level
            switch (mainwin.osdType)
            {
                case OSDType.Local:
                    DrawVolumeBar(gr,
                        "<< " + mainwin.appmon.FriendlyName + " >>\nTable Volume   " 
                        + Math.Round(mainwin.localVolume * 100) + "%" + muted,
                        mainwin.localVolume, r);
                    break;

                case OSDType.Local2:
                    DrawVolumeBar(gr,
                        "<< " + mainwin.appmon.FriendlyName + " >>\nTable Volume (Secondary)   "
                        + Math.Round(mainwin.local2Volume * 100) + "%" + muted,
                        mainwin.local2Volume, r);
                    break;

                case OSDType.Global:
                case OSDType.None:
                    var v = mainwin.globalVolume[(int)mainwin.volumeMode];
                    DrawVolumeBar(gr,
                        "Global Volume   " + Math.Round(v * 100) + "%" + muted, v, r);
                    break;

                case OSDType.SSFBG:
                    DrawVolumeBar(gr,
                        "<< " + mainwin.appmon.FriendlyName + " >>\nSSF Back Glass Gain   "
                        + Math.Round(mainwin.SSFBGVolume) + "db" + muted,
                        mainwin.SSFBGVolume, r);
                    break;

                case OSDType.SSFRS:
                    DrawVolumeBar(gr,
                        "<< " + mainwin.appmon.FriendlyName + " >>\nSSF Rear Exciters Gain   "
                        + Math.Round(mainwin.SSFRSVolume) + "db" + muted,
                        mainwin.SSFRSVolume, r);
                    break;

                case OSDType.SSFFS:
                    DrawVolumeBar(gr,
                        "<< " + mainwin.appmon.FriendlyName + " >>\nSSF Front Exciters Gain   "
                        + Math.Round(mainwin.SSFFSVolume) + "db" + muted,
                        mainwin.SSFFSVolume, r);
                    break;
            }
        }

        void DrawVolumeBar(Graphics gr, String title, float vol, int r)
        {
            // Get the window size.  If rotated 90 or 270 degrees, swap width
            // and height for our bar size calculations.
            Size winsz = ClientSize;
            if (r == 90 || r == 270)
            {
                winsz.Width = ClientSize.Height;
                winsz.Height = ClientSize.Width;
            }

            // Figure the bar height, based on the text height
            SizeF txtsz = gr.MeasureString("X", font);
            Size barsz = new Size(winsz.Width, (int)(txtsz.Height * 1.5f));

            // Figure the bar top position, aligning at the bototm
            int left = 0, right = barsz.Width;
            int top = winsz.Height - barsz.Height;

            // figure the volume bar width in pixels
            int volwid = (int)(barsz.Width * vol);

            // Start with a default tick width, then refigure so that we fit an
            // integral number (or as close as possible) into the available width.
            int tickwid = 30, ticksp = 10;
            int availwid = winsz.Width - tickwid;
            int nticks = Math.Max(availwid / (tickwid + ticksp), 1);
            tickwid = Math.Max((availwid / nticks) - ticksp, 1);

            // select the brushes
            Brush black = Brushes.Black;
            Brush rcbrush, txbrush;
            switch (mainwin.osdType)
            {
                case OSDType.Local:
                    rcbrush = txbrush = Brushes.Lime;
                    break;

                case OSDType.Local2:
                    rcbrush = txbrush = Brushes.Violet;
                    break;

                case OSDType.SSFBG:
                case OSDType.SSFRS:
                case OSDType.SSFFS:
                    rcbrush = txbrush = Brushes.White;
                    break;

                case OSDType.Global:
                case OSDType.None:
                default:
                    if (mainwin.volumeMode == UIWin.VolumeMode.Night)
                        rcbrush = txbrush = Brushes.Blue;
                    else
                        rcbrush = txbrush = Brushes.DeepSkyBlue;
                    break;
            }

            // create an outline pen if needed
            bool mute = mainwin.globalMute;
            int penwid = 2;
            using (Pen rcpen = mute ? new Pen(rcbrush, penwid) : null)
            {
                // draw the ticks
                for (int x = left; x < right; x += tickwid + ticksp)
                {
                    // get the area of this tick
                    Rectangle rc;
                    Rectangle halftick = new Rectangle(x + tickwid / 4, top + barsz.Height / 4, tickwid / 2, barsz.Height / 2);
                    bool drawhalf = false;
                    if (x + tickwid <= volwid)
                    {
                        // we're still below the volume level, so draw a whole tick
                        rc = new Rectangle(x, top, tickwid, top + barsz.Height);
                    }
                    else if (x < volwid)
                    {
                        // this tick is partially within the volume level, so draw a portion
                        // of the tick plus the small tick
                        rc = new Rectangle(x, top, volwid - x, top + barsz.Height);
                        if (rc.Right > halftick.Left)
                            halftick = new Rectangle(rc.Right, halftick.Top, halftick.Right - rc.Right, halftick.Height);
                        drawhalf = true;
                    }
                    else
                    {
                        // We're entirely beyond the volume level.  Draw a half tick.
                        rc = halftick;
                    }

                    // draw the rectangle or outline
                    if (mute)
                        gr.DrawRectangle(rcpen, rc);
                    else
                        gr.FillRectangle(rcbrush, rc);

                    // if we have a partial tick, also draw the half tick
                    if (drawhalf)
                    {
                        if (mute)
                            gr.DrawRectangle(rcpen, halftick);
                        else
                            gr.FillRectangle(rcbrush, halftick);
                    }
                }
            }

            // draw the title overlay
            float tx = left + (right - left) / 2;
            float ty = top - 8;
            gr.DrawString(title, font, black, tx + 1, ty + 1, titleFmt);    // shadow
            gr.DrawString(title, font, txbrush, tx, ty, titleFmt);          // main text

            // add the night mode icon if appropriate
            if (mainwin.volumeMode == UIWin.VolumeMode.Night)
            {
                SizeF titlesz = gr.MeasureString(title, font);
                SizeF linesz = gr.MeasureString("X", font);
                float psz = linesz.Height * .8f;
                float px = tx - titlesz.Width/2 - 8 - psz;
                float py = ty - titlesz.Height/2 - psz/2;
                gr.DrawImage(nightMode, new PointF[] { 
                    new PointF(px, py), 
                    new PointF(px + psz, py),
                    new PointF(px, py + psz)
                });
            }
        }

        // overlay font
        Font font;
        StringFormat titleFmt;

        // night mode image
        Bitmap nightMode = PinVol.Properties.Resources.nightModeLarge;

        private void OSDWin_Resize(object sender, EventArgs e)
        {
            Invalidate();
            btnCCW.Top = ClientSize.Height - btnCCW.Height;
            btnCW.Top = ClientSize.Height - btnCW.Height;
            btnCW.Left = ClientSize.Width - btnCW.Width;
            btnDone.Left = (ClientSize.Width - btnDone.Width) / 2;
            btnDone.Top = (ClientSize.Height - btnDone.Height) / 2;

            SavePos();
        }

        private void OSDWin_Load(object sender, EventArgs e)
        {
            Rectangle r = mainwin.cfg.OSDPos;
            Location = new Point(r.Left, r.Top);
            ClientSize = new Size(r.Width, r.Height);
        }

        private void btnCCW_Click(object sender, EventArgs e)
        {
            RotateBy(90);
        }

        private void btnCW_Click(object sender, EventArgs e)
        {
            RotateBy(-90);
        }

        // Rotate by the given angle.  Note that we only support display 
        // rotations that are multiples of 90, so the delta must also be 
        // a multiple of 90.
        private void RotateBy(int dAngle)
        {
            // apply the delta, limiting to the 0-360 range
            mainwin.cfg.OSDRotation = ((mainwin.cfg.OSDRotation + dAngle) % 360 + 360) % 360;

            // note the config update
            mainwin.SetCfgDirty();

            // if rotating by +/- 90, swap the window's height and width
            if (Math.Abs(dAngle) % 180 == 90)
            {
                // note the current bottom location
                Point br = new Point(Right, Bottom);

                // rotate it
                int h = Height;
                Height = Width;
                Width = h;

                // The rotation controls are at the bottom of the window,
                // so it seems most intuitive to rotate around the bottom
                // right.  Reposition the window so that the bottom right
                // stays where it was.
                Left = br.X - Width;
                Top = br.Y - Height;

                // ...but make sure the title bar is still on screen
                Rectangle scr = Screen.FromControl(this).Bounds;
                if (Top < scr.Top)
                    Top = scr.Top;
                if (Left < scr.Left)
                    Left = scr.Left;
            }

            // make sure we redraw the interior at the new rotation
            Invalidate();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Setup(false);
        }

        private void OSDWin_Move(object sender, EventArgs e)
        {
            SavePos();
        }

        void SavePos()
        {
            if (setupMode)
            {
                Point pos = PointToScreen(new Point(0, 0));
                mainwin.cfg.OSDPos = new Rectangle(pos.X, pos.Y, ClientSize.Width, ClientSize.Height);
                mainwin.SetCfgDirty();
            }
        }
    }
}
