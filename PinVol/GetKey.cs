using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;

namespace PinVol
{
    public partial class GetKey : Form
    {
        public GetKey(UIWin.KeyField key, bool openedByKey)
        {
            // initialize the form
            InitializeComponent();

            // set up to read raw keystrokes
            kbhook = new GlobalKeyboardHook();
            kbhook.KeyboardPressed += OnKeyPressed;

            // eat the first event if opened by a keystroke
            eatEvent = openedByKey;

            // set up to receive joystick button presses
            JoystickDev.JoystickButtonChanged += OnJoystickButtonDown;
        }

        // skip an event - used if we're opening the dialog via a keystroke,
        bool eatEvent;

        // results
        public bool hasKey = false;
        public Keys key = Keys.None;
        public bool shift = false, control = false, alt = false, windows = false;
        public Guid joystick = Guid.Empty;
        public int joystickButton = -1;

        public void OnJoystickButtonDown(object sender, JoystickDev.JoystickEventArgs ev)
        {
            // if it's a button-down event, use it
            if (ev.down)
            {
                // set the result fields to indicate a joystick button
                hasKey = true;
                key = Keys.None;
                joystick = (sender as JoystickDev).instanceGuid;
                joystickButton = ev.button;

                // close the dialog
                Close();
            }
        }

        private GlobalKeyboardHook kbhook;
        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            // check to see if we're eating the first key press
            if (eatEvent)
            {
                eatEvent = false;
                return;
            }


            // check for modifier keys
            Keys k = (Keys)e.KeyboardData.VirtualCode;
            switch (k)
            {
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LWin:
                case Keys.RWin:
                case Keys.LMenu:
                case Keys.RMenu:
                case Keys.Menu:
                case Keys.Alt:
                    // these are all modifiers - ignore them
                    return;
            }

            // set the result fields to indicate the keystroke
            hasKey = true;
            key = k;
            UpdateModifiers();

            // close the dialog
            Close();

            // mark the event as handled to suppress any further handling for the key
            e.Handled = true;
        }

        void UpdateModifiers()
        {
            // read the current states of the modifier keys
            shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            control = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            windows = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);

            // update the on-screen display to match the current keyboard state
            Color on = Color.White, off = Color.DimGray;
            lblShift.ForeColor = shift ? on : off;
            lblControl.ForeColor = control ? on : off;
            lblAlt.ForeColor = alt ? on : off;
            lblWindows.ForeColor = windows ? on : off;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void GetKey_Load(object sender, EventArgs e)
        {
        }

        private void keyStateTimer_Tick(object sender, EventArgs e)
        {
            UpdateModifiers();
        }

        private void GetKey_FormClosed(object sender, FormClosedEventArgs e)
        {
            kbhook.Dispose();
            kbhook = null;
            base.Close();
        }
    }
}
