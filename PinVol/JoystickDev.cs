using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectInput;
using System.Windows.Forms;
using System.Diagnostics;

namespace PinVol
{
    public class JoystickDev
    {
        public JoystickDev(int unitNo, Joystick js, Guid instanceGuid)
        {
            this.unitNo = unitNo;
            this.js = js;
            this.instanceGuid = instanceGuid;
            this.nButtons = js.Capabilities.ButtonCount;
            this.hWait = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.productName = js.Information.ProductName.TrimEnd('\0');
            this.isPinscape = Regex.IsMatch(this.productName, @"Pinscape\s*Controller");
        }

        // our DirectInput instance
        public static DirectInput directInput;

        // UI window
        public static UIWin win;

        // joystick object data
        public int unitNo;              // our internal unit number
        public Joystick js;             // DirectInput Joystick object
        public Guid instanceGuid;       // instance GUID of Windows HID
        public String productName;      // product name string from device
        public int nButtons;            // number of buttons
        public WaitHandle hWait;        // event wait handle, for state change notifications
        public Thread thread;           // state monitor thread
        public bool isPinscape;         // is this a Pinscape controller?

        // Get the unit name for display purposes.  If there's only one joystick
        // in the system, the name is simply "Joystick".  Otherwise, it's 
        // "Joystick n", where n is the unit number in our internal list order.
        public String UnitName
        {
            get { return joysticks.Count > 1 ? "Joystick " + unitNo : "Joystick"; }
        }

        // list of attached joysticks
        public static Dictionary<Guid,JoystickDev> joysticks = new Dictionary<Guid,JoystickDev>();

        // look up a joystick by GUID
        public static JoystickDev FromGuid(Guid guid)
        {
            return joysticks.ContainsKey(guid) ? joysticks[guid] : null;
        }

        // number of active joysticks
        public static int Count
        {
            get { return joysticks.Count; }
        }

        // Enable joystick handling
        public static void Enable(UIWin win)
        {
            // do nothing if already enable
            if (directInput != null)
                return;

            // remember the UI window
            JoystickDev.win = win;
            
            // get our DirectInput object
            directInput = new DirectInput();

            // clear the thread exit event
            exitThreadsEvent.Reset();

            // scan for devices
            Rescan();
        }

        // Rescan joysticks.  This can be called after a new joystick has been
        // plugged in, or just on spec that one might have.  We'll run the system
        // discovery scan again, and add new entries to our internal list for any
        // joysticks not already in the list.
        public static void Rescan()
        {
            // log scans for debugging purposes?
            bool log = false;
            if (log)
                Log.Info("Scanning for USB devices");

            // get the list of attached joysticks
            var devices = directInput.GetDevices(DeviceClass.All, DeviceEnumerationFlags.AttachedOnly);
            int unitNo = joysticks.Count + 1;
            foreach (var dev in devices)
            {
                // if this device is already in our list, there's nothing to do
                if (joysticks.ContainsKey(dev.InstanceGuid))
                    continue;

                // check USB usage to see if it's a joystick
                bool isJoystick = (dev.UsagePage == SharpDX.Multimedia.UsagePage.Generic 
                    && dev.Usage == SharpDX.Multimedia.UsageId.GenericJoystick);

                // check if it's a gamepad
                bool isGamepad = (dev.UsagePage == SharpDX.Multimedia.UsagePage.Generic
                    && dev.Usage == SharpDX.Multimedia.UsageId.GenericGamepad);

                // note the product name for logging purposes
                String productName = dev.ProductName.TrimEnd('\0');

                // log it if desired
                if (log)
                {
                    Log.Info((isJoystick ? "  Found joystick: " : isGamepad ? "  Found gamepad: " : "  Found non-joystick device: ")
                        + productName
                        + ", DirectInput type=" + dev.Type + "/" + dev.Subtype
                        + ", USB usage=" + (int)dev.UsagePage + "." + (int)dev.Usage);
                }

                // skip devices that aren't joysticks or gamepads
                if (!isJoystick && !isGamepad)
                    continue;

                // initialize it
                try
                {
                    // create the instance and add it to our list
                    Joystick js = new Joystick(directInput, dev.InstanceGuid);
                    JoystickDev jsdev = new JoystickDev(unitNo++, js, dev.InstanceGuid);
                    joysticks[dev.InstanceGuid] = jsdev;

                    // request non-exclusive background access
                    js.SetCooperativeLevel(win.Handle, CooperativeLevel.NonExclusive | CooperativeLevel.Background);

                    // Set up an input monitor thread for the joystick.  Note that the
                    // DX API requires this to be done before we call 'Acquire'.
                    js.SetNotification(jsdev.hWait);

                    // connect to the joystick
                    js.Acquire();

                    // Start the monitor thread.  Note that we have to do this after
                    // calling 'Acquire', since the thread will want to read state.
                    jsdev.thread = new Thread(jsdev.JoystickThreadMain);
                    jsdev.thread.Start();
                }
                catch (Exception ex)
                {
                    Log.Error("  !!! Error initializing joystick device " + productName + ": " + ex.Message);
                }
            }
        }

        // Log the joystick list
        public static void LogDeviceList()
        {
            Log.Info("Joystick devices discovered:" + (joysticks.Count == 0 ? " None" : ""));
            foreach (var kv in joysticks)
            {
                var js = kv.Value;
                Log.Info("   #" + js.unitNo + " " + js.productName + " (" + js.nButtons + " buttons)");
            }
        }

        // Disable joystick handling
        public static void Disable()
        {
            // do nothing if already disabled
            if (directInput == null)
                return;

            // terminate the monitor threads
            exitThreadsEvent.Set();

            // dispose of joystick instances
            foreach (var kv in joysticks)
            {
                // wait for the monitor thread to exit
                var js = kv.Value;
                js.thread.Join(2500);

                // dispose of the DirectInput object
                js.js.Dispose();
                js.js = null;
            }

            // forget the joystick objects
            joysticks.Clear();

            // dispose of our DirectInput object
            directInput.Dispose();
            directInput = null;

            // forget the UI window
            win = null;
        }

        // clean up before program exit
        public static void Cleanup()
        {
            Disable();
        }

        // joystick button change event
        public static event JoystickEventHandler JoystickButtonChanged;

        // joystick event handling
        public delegate void JoystickEventHandler(object sender, JoystickEventArgs ev);
        public class JoystickEventArgs : EventArgs
        {
            public JoystickEventArgs(int button, bool down, bool repeat)
            {
                this.button = button;
                this.down = down;
                this.repeat = repeat;
            }
            public int button;      // button being pressed or released
            public bool down;       // true -> button is down, false -> button is up
            public bool repeat;     // this is an auto-repeat event, not a state change
        }

        // Joystick button state
        class ButtonState
        {
            public bool down = false;           // is the key down?
            public DateTime repeat;             // time for next autorepeat press event
        }

        // Joystick monitor thread.  We start one thread per joystick to 
        // monitor the joystick's state change event.
        private static EventWaitHandle exitThreadsEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private void JoystickThreadMain()
        {
            // get the current state
            var state = new JoystickState();
            try
            {
                js.GetCurrentState(ref state);
            }
            catch
            {
                // An error occurred.  The joystick might have been unplugged.
                // Tell the main loop to rebuild the joystick list, then exit
                // the thread.
                win.BeginInvoke((Action)delegate { win.OnJoystickError(this); });
                return;
            }

            // start with all buttons off
            var buttons = new ButtonState[state.Buttons.Length].Select(b => new ButtonState()).ToArray();

            // wait handles - we wait for a joystick state change or an exit signal
            var handles = new WaitHandle[] { hWait, exitThreadsEvent };

            // figure the key repeat time parameters in milliseconds
            int keyDelay = (SystemInformation.KeyboardDelay + 1) * 250;
            int keySpeed = (int)Math.Round(1000.0 / (SystemInformation.KeyboardSpeed + 2.5));

            // Key event invoker generator.  We need to dispatch events in a loop over
            // the buttons, so we need to generate the invoker closure with a call to
            // a nested function.  The nested function call captures the current button
            // index as a parameter to the function; if we didn't do this, the index
            // in the invoker would bind to the live local index variable in this method,
            // which can change by the time the handler is actually invoked.
            Func<int,bool,Action> invoker = (i, repeat) =>
            {
                var handler = JoystickButtonChanged;
                return delegate { 
                    if (handler != null)
                        handler(this, new JoystickEventArgs(i, buttons[i].down, repeat)); 
                };
            };

            // monitor the joystick
            while (true)
            {
                // Figure the next timeout time.  If any buttons are down, we'll
                // time out at the earliest auto-repeat time.  Otherwise we'll
                // wait indefinitely.
                DateTime endTime = buttons.Min(b => b.down ? b.repeat : DateTime.MaxValue);
                DateTime now = DateTime.Now;
                int timeout = endTime == DateTime.MaxValue ? Timeout.Infinite :
                    endTime < now ?  0 :
                    (int)(endTime - DateTime.Now).TotalMilliseconds;

                // wait for an event
                switch (WaitHandle.WaitAny(handles, timeout))
                {
                    case WaitHandle.WaitTimeout:
                        break;

                    case 0:
                        // joystick event
                        JoystickEventHandler handler = JoystickButtonChanged;
                        if (handler != null)
                        {
                            // get the current joystick state
                            try
                            {
                                js.GetCurrentState(ref state);
                            }
                            catch
                            {
                                // An error occurred.  The joystick might have been unplugged.
                                // Tell the main loop to rebuild the joystick list, then exit
                                // the thread.
                                win.BeginInvoke((Action)delegate { win.OnJoystickError(this); });
                                return;
                            }

                            // Ignore events from Pinscape devices when the Pinscape config
                            // tool is running.  The device sends special status reports to
                            // the config tool while it's running; these are piggybacked on
                            // the joystick interface, so they look like random garbage to
                            // joystick readers.
                            if (isPinscape && Program.configToolRunning)
                                break;

                            // scan for button changes
                            for (int i = 0; i < buttons.Length; ++i)
                            {
                                // if the button has changed, update it and fire an event
                                if (buttons[i].down != state.Buttons[i])
                                {
                                    // Set the new button state internally
                                    buttons[i].down = state.Buttons[i];

                                    // If the button is newly down, set the first auto-repeat
                                    // interval
                                    if (state.Buttons[i])
                                        buttons[i].repeat = DateTime.Now.AddMilliseconds(keyDelay);

                                    // Fire the event.  Note that we need to use Invoke to post the
                                    // event to the UI thread.
                                    win.BeginInvoke(invoker(i, false));
                                }
                            }
                        }
                        break;

                    case 1:
                        // thread exit event
                        return;
                }
                
                // Fire any autorepeat key events.  Skip this if the config tool is
                // running, for the same reason we don't send primary key events.
                if (!(isPinscape && Program.configToolRunning))
                {
                    var t = DateTime.Now;
                    for (int i = 0; i < buttons.Length; ++i)
                    {
                        var b = buttons[i];
                        if (b.down && t >= b.repeat)
                        {
                            // fire the event
                            win.BeginInvoke(invoker(i, true));

                            // set the next repeat time
                            b.repeat = DateTime.Now.AddMilliseconds(keySpeed);
                        }
                    }
                }
            }
        }

    }
}
