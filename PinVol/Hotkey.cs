using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

namespace PinVol
{
    public class Hotkey : IMessageFilter
    {
        #region Interop

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int UnregisterHotKey(IntPtr hWnd, int id);

        private const uint WM_HOTKEY = 0x312;

        private const uint MOD_ALT = 0x1;
        private const uint MOD_CONTROL = 0x2;
        private const uint MOD_SHIFT = 0x4;
        private const uint MOD_WIN = 0x8;
        private const uint MOD_NOREPEAT = 0x4000;

        private const uint ERROR_HOTKEY_ALREADY_REGISTERED = 1409;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern ushort GetAsyncKeyState(int vkey);

        private const uint KEY_DOWN = 0x8000;

        #endregion

        private static int currentID;
        private const int maximumID = 0xBFFF;

        private Keys keyCode;
        private bool shift;
        private bool control;
        private bool alt;
        private bool windows;
        private bool autoRepeat = true;

        [XmlIgnore]
        private int id;
        [XmlIgnore]
        private bool registered;
        [XmlIgnore]
        private Control windowControl;

        public event HandledEventHandler Pressed;

        public Hotkey()
            : this(Keys.None, false, false, false, false)
        {
            // No work done here!
        }

        public Hotkey(Config.KeyInfo key)
        {
            Init(key.key, key.shift, key.control, key.alt, key.windows);
        }

        public Hotkey(Keys keyCode, bool shift, bool control, bool alt, bool windows)
        {
            Init(keyCode, shift, control, alt, windows);
        }

        private void Init(Keys keyCode, bool shift, bool control, bool alt, bool windows)
        {
            // Assign properties
            Set(keyCode, shift, control, alt, windows);

            // Register us as a message filter
            Application.AddMessageFilter(this);

            // set up the auto-repeat timer in case we need it
            autoRepeatTimer.Interval = 5;
            autoRepeatTimer.Tick += autoRepeatTimer_Tick;
        }

        public void Set(Keys keyCode, bool shift, bool control, bool alt, bool windows)
        {
            this.KeyCode = keyCode;
            this.Shift = shift;
            this.Control = control;
            this.Alt = alt;
            this.Windows = windows;
        }
        
        public void Set(Config.KeyInfo key)
        {
            Set(key.key, key.shift, key.control, key.alt, key.windows);
        }

        ~Hotkey()
        {
            // Unregister the hotkey if necessary
            if (this.Registered)
                this.Unregister();
        }

        public Hotkey Clone()
        {
            // Clone the whole object
            return new Hotkey(this.keyCode, this.shift, this.control, this.alt, this.windows);
        }

        public bool GetCanRegister(Control windowControl)
        {
            // Handle any exceptions: they mean "no, you can't register" :)
            try
            {
                // Attempt to register
                if (!this.Register(windowControl))
                    return false;

                // Unregister and say we managed it
                this.Unregister();
                return true;
            }
            catch (Win32Exception)
            { 
                return false; 
            }
            catch (NotSupportedException)
            { 
                return false; 
            }
        }

        public bool Register(Control windowControl)
        {
            // Check that we have not registered
            if (this.registered)
                throw new NotSupportedException("You cannot register a hotkey that is already registered");

            // We can't register an empty hotkey
            if (this.Empty)
                throw new NotSupportedException("You cannot register an empty hotkey");

            // Get an ID for the hotkey and increase current ID
            this.id = Hotkey.currentID;
            Hotkey.currentID = (Hotkey.currentID + 1) % Hotkey.maximumID;

            // Translate modifier keys into unmanaged version
            uint modifiers = (this.Alt ? Hotkey.MOD_ALT : 0) | (this.Control ? Hotkey.MOD_CONTROL : 0) |
                            (this.Shift ? Hotkey.MOD_SHIFT : 0) | (this.Windows ? Hotkey.MOD_WIN : 0);

            // Register the hotkey
            if (Hotkey.RegisterHotKey(windowControl.Handle, this.id, modifiers, keyCode) == 0)
            {
                // Is the error that the hotkey is registered?
                if (Marshal.GetLastWin32Error() == ERROR_HOTKEY_ALREADY_REGISTERED)
                    return false;
                else
                    throw new Win32Exception();
            }

            // Save the control reference and register state
            this.registered = true;
            this.windowControl = windowControl;

            // We successfully registered
            return true;
        }

        public void Unregister()
        {
            // Check that we have registered
            if (!this.registered)
                return;

            // It's possible that the control itself has died: in that case, no need to unregister!
            if (!this.windowControl.IsDisposed)
            {
                // Clean up after ourselves
                if (Hotkey.UnregisterHotKey(this.windowControl.Handle, this.id) == 0)
                    throw new Win32Exception();
            }

            // Clear the control reference and register state
            this.registered = false;
            this.windowControl = null;
        }

        private void Reregister()
        {
            // Only do something if the key is already registered
            if (!this.registered)
                return;

            // Save control reference
            Control windowControl = this.windowControl;

            // Unregister and then reregister again
            this.Unregister();
            this.Register(windowControl);
        }

        public bool PreFilterMessage(ref Message message)
        {
            // Only process WM_HOTKEY messages
            if (message.Msg != Hotkey.WM_HOTKEY)
                return false;

            // Check that the ID is our key and we are registerd
            if (this.registered && (message.WParam.ToInt32() == this.id))
            {
                // Fire the event and pass on the event if our handlers didn't handle it
                return this.OnPressed();
            }
            else
                return false;
        }

        private bool OnPressed()
        {
            // If the auto-repeat option is off, and this is an auto-repeat
            // event, ignore it.  It's an auto-repeat event if our 'down'
            // flag is set.
            if (!autoRepeat && down)
                return false;
            
            // fire the event if we can
            HandledEventArgs handledEventArgs = new HandledEventArgs(false);
            if (this.Pressed != null)
                this.Pressed(this, handledEventArgs); 

            // If the key is newly down, and we're NOT in auto-repeat mode,
            // mark the key state as down and start the key polling timer.
            if (!autoRepeat && !down)
            {
                down = true;
                autoRepeatTimer.Enabled = true;
            }

            // Return whether we handled the event or not
            return handledEventArgs.Handled;
        }

        void autoRepeatTimer_Tick(object sender, EventArgs ev)
        { 
            // check the key state
            uint state = GetAsyncKeyState((int)keyCode);

            // if the key is no longer down, we can stop monitoring it
            if ((state & KEY_DOWN) == 0)
            {
                down = false;
                autoRepeatTimer.Enabled = false;
            }
        }

        public override string ToString()
        {
            // We can be empty
            if (this.Empty)
                return "(none)";

            // Build key name
            string keyName = Enum.GetName(typeof(Keys), this.keyCode); ;
            switch (this.keyCode)
            {
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                    // Strip the first character
                    keyName = keyName.Substring(1);
                    break;

                default:
                    // Leave everything alone
                    break;
            }

            // Build modifiers
            string modifiers = "";
            if (this.shift)
                modifiers += "Shift+";
            if (this.control)
                modifiers += "Control+";
            if (this.alt)
                modifiers += "Alt+";
            if (this.windows)
                modifiers += "Windows+";

            // Return result
            return modifiers + keyName;
        }

        public bool Empty
        {
            get { return this.keyCode == Keys.None; }
        }

        public bool Registered
        {
            get { return this.registered; }
        }

        public Keys KeyCode
        {
            get { return this.keyCode; }
            set
            {
                // Save and reregister
                this.keyCode = value;
                this.Reregister();
            }
        }

        public bool Shift
        {
            get { return this.shift; }
            set
            {
                // Save and reregister
                this.shift = value;
                this.Reregister();
            }
        }

        public bool Control
        {
            get { return this.control; }
            set
            {
                // Save and reregister
                this.control = value;
                this.Reregister();
            }
        }

        public bool Alt
        {
            get { return this.alt; }
            set
            {
                // Save and reregister
                this.alt = value;
                this.Reregister();
            }
        }

        public bool Windows
        {
            get { return this.windows; }
            set
            {
                // Save and reregister
                this.windows = value;
                this.Reregister();
            }
        }

        public bool AutoRepeat
        {
            get { return this.autoRepeat; }
            set
            {
                this.autoRepeat = value;
                this.Reregister();
            }
        }

        // By default, the Windows hotkey mechanism delivers events on every
        // auto-repeat key press when the key is held down.  This means we need
        // to suppress the auto-repeat events if we're NOT in auto-repeat mode.
        // On Windows 7+, we can do this by setting the MOD_NOREPEAT flag in
        // the key modifers when registering the key, but this causes the
        // registration to fail on XP and Vista.  For the sake of those, we'll
        // suppress the auto-repeat keys with our own mechanism that doesn't
        // depend on MOD_NOREPEAT.  Here's how.  We'll set up a timer to poll
        // the key state every few milliseconds.  When the key is first pressed,
        // we'll set the 'down' flag here and enable the timer.  When the timer
        // sees the key state change to key-up, it'll clear the 'down' flag and
        // stop the timer.  In the event handler, we'll ignore any key press
        // events as long as the 'down' flag is set, since they must be auto-
        // repeat events.
        public Timer autoRepeatTimer = new Timer();
        public bool down;
    }
}
