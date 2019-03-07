using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SharpDX;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;
using System.Reflection;


namespace PinVol
{
    public partial class UIWin : Form, IAudioEndpointVolumeCallback
    {
        public UIWin()
        {
            // initialize the form
            InitializeComponent();

            // set up the foreground application monitor
            appmon = new AppMonitor();
        }

        // per-application volume data list
        class LocalVol
        {
            public LocalVol() { primary = secondary = 1.0f; }
            public LocalVol(float p) { primary = p; secondary = 1.0f; }
            public LocalVol(float p, float s) { primary = p; secondary = s; }
            public float primary;       // primary audio device local volume
            public float secondary;     // secondary audio device volume, as a fraction of primary
        }
        Dictionary<String, LocalVol> appVol = new Dictionary<String, LocalVol>();

        // path to context volume settings files
        String dbPath = "PinVolTables.ini";

        // path to global volume settings file
        String volPath = "PinVolVol.ini";

        // Load the saved volume data
        void LoadSavedVols()
        {
            // try loading the global volume level file
            if (File.Exists(volPath))
            {
                try
                {
                    // read the file contents and parse it
                    String[] lines = File.ReadAllLines(volPath);
                    for (int lineno = 0 ; lineno < lines.Length ; ++lineno)
                    {
                        // skip comments
                        if (Regex.IsMatch(lines[lineno], @"^\s*(#|$)"))
                            continue;
                        
                        // check for the volume level line
                        Match m = Regex.Match(lines[lineno], @"(?i)^\s*(global|night|default)\s*=\s*(\d+)\s*#?");
                        if (m.Success)
                        {
                            // parse out the groups and store the day/night global volume level
                            String type = m.Groups[1].Value.ToLower();;
                            float val = LimitVolume(int.Parse(m.Groups[2].Value)/100.0f);
                            switch (type)
                            {
                                case "global":
                                    globalVolume[0] = val;
                                    break;

                                case "night":
                                    globalVolume[1] = val;
                                    break;

                                case "default":
                                    defaultVolume = val;
                                    break;
                            }

                            // handled
                            continue;
                        }

                        // unrecognized syntax
                        Log.Error("Invalid syntax in global volume level file (" + volPath + ") at line " + (lineno + 1));
                    }
                }
                catch (Exception exc)
                {
                    Log.Error("Error reading the global volume settings file (" + volPath + "): " + exc.Message);
                }
            }

            // load or create the table volume level database
            if (File.Exists(dbPath))
            {
                String[] lines = null;
                try
                {
                    // read the file contents and parse it
                    lines = File.ReadAllLines(dbPath);
                }
                catch (Exception exc)
                {
                    MessageBox.Show("An error occurred trying to read the PinVol settings file ("
                        + exc.Message + ")."
                        + "\n\n"
                        + "You can still run PinVol, but it won't be able to do one of its main jobs, "
                        + "namely restoring saved settings from past sessions, since that information "
                        + "was supposed to come from this file. Plus, chances are that something is wrong "
                        + "with the whole PinVol installation if it can't access its own files, so "
                        + "please check that the program is installed in a folder where you have "
                        + "read/write permission.");
                }
                if (lines != null)
                {
                    for (int lineno = 0; lineno < lines.Length; ++lineno)
                    {
                        try
                        {
                            // get this line; skip blank lines and comments ("#" lines)
                            String line = lines[lineno];
                            if (Regex.IsMatch(line, @"^\s*(#|$)"))
                                continue;

                            // app lines have the format <application name><tab><level as percentage>
                            Match m = Regex.Match(line, @"^([^\t]*)\t(\d+)(?:\t(\d+))?$");
                            if (m.Success)
                            {
                                float primary = int.Parse(m.Groups[2].Value) / 100.0f;
                                float secondary = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) / 100.0f : primary;
                                appVol[m.Groups[1].Value] = new LocalVol(primary, secondary);
                                continue;
                            }

                            // unmatched syntax - log an error
                            Log.Error("Invalid syntax in volume database at line " + (lineno + 1));
                        }
                        catch (Exception exc)
                        {
                            Log.Error("Error parsing volume database at line " + (lineno + 1)
                                + ": " + exc.Message);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    File.WriteAllLines(dbPath, new String[]{
                        "# PinVol table volume level list",
                        "# Created " + DateTime.Now.ToString(),
                        "# Levels are percentages relative to global volume setting"
                    });
                }
                catch (Exception exc)
                {
                    // This reduces our functionality so much that we should let the user know
                    // directly, rather than expecting them to find the error in the log.
                    MessageBox.Show("The PinVol settings file (" + dbPath + ") doesn't exist,"
                        + " and an error occurred trying to create it (" + exc.Message + ")."
                        + "\n\n"
                        + "You can still run PinVol, but it won't be able to do the main thing "
                        + "it's designed for, which is to remember and restore individual "
                        + "audio volume level settings per pinball table. To do that, we "
                        + "need to be able to save the settings in this file. Please check "
                        + "that PinVol is installed in a folder where you have write permission.");
                    Log.Error("The settings database file doesn't exist, and an error occurred trying to create it: " + exc.Message);
                }
            }
        }

        // Save the global volume settings
        int globalVolSaveErrCnt = 0;
        void SaveGlobalVols()
        {
            try
            {
                File.WriteAllLines(volPath, new String[]{
                    "# PinVol volume levels",
                    "# Saved " + DateTime.Now.ToString(),
                    "Global = " + Math.Round(globalVolume[0]*100),
                    "Night = " + Math.Round(globalVolume[1]*100),
                    "Default = " + Math.Round(defaultVolume*100)
                });
            }
            catch (Exception exc)
            {
                if (globalVolSaveErrCnt++ < 5)
                    Log.Error("Error saving the global volume settings: " + exc.Message);
            }
        }

        // Save the local volume database
        int localVolSaveErrCnt = 0;
        void SaveLocalVols()
        {
            // set up the list with header comments
            List<String> lines = new List<String>();
            lines.Add("# PinVol volume levels list");
            lines.Add("# Saved " + DateTime.Now.ToString());
            lines.Add("");

            // add the volume data
            foreach (KeyValuePair<String, LocalVol> p in appVol)
                lines.Add(p.Key + "\t" + Math.Round(p.Value.primary * 100.0f).ToString()
                    + "\t" + Math.Round(p.Value.secondary * 100.0f).ToString());

            // write it out
            try
            {
                File.WriteAllLines(dbPath, lines.ToArray());
            }
            catch (Exception exc)
            {
                if (localVolSaveErrCnt++ < 5)
                    Log.Error("Error saving the table volume database: " + exc.Message);
            }
        }

        // Save the current global volume levels
        void SetGlobalVol(float vol)
        {
            vol = LimitVolume(vol);
            if (vol != globalVolume[(int)volumeMode])
            {
                globalVolume[(int)volumeMode] = vol;
                SetGlobalVolDirty();
            }
        }

        // current foreground application or table
        String curApp;

        // Save the current local volume for the current app/table in the database
        void SetLocalVol(LocalVol vol) 
        { 
            SetLocalVol(vol.primary, vol.secondary);
        }
        void SetLocalVol(float primary, float secondary)
        {
            // update the internal local volume
            localVolume = primary = LimitVolume(primary);
            local2Volume = secondary = LimitVolume(secondary);

            // update the local database if it's a change to the stored value
            if (curApp != null)
            {
                LocalVol v;
                if (!appVol.ContainsKey(curApp))
                {
                    appVol[curApp] = new LocalVol(primary, secondary);
                    SetLocalVolDirty();
                }
                else if ((v = appVol[curApp]).primary != primary || v.secondary != secondary)
                {
                    v.primary = primary;
                    v.secondary = secondary;
                    SetLocalVolDirty();
                }
            }
        }

        // foreground application monitor
        public AppMonitor appmon;

        // volume OSD window
        OSDWin osdwin;

        // volume type last shown in OSD window
        public OSDWin.OSDType osdType = OSDWin.OSDType.Local;

        // time to turn off the OSD
        DateTime osdOffTime = DateTime.Now;

        // Our current volume levels.  The global volume is an absolute level
        // that applies to all appliations and pinball tables.  The local level
        // is a percentage of the global level that applies to the currently
        // active application/table.  The system volume setting at any given
        // time is the global level times the local level.
        //
        // There are actually two global volume levels, one per mode.  Mode 0
        // is day mode, mode 1 is night mode.  'volumeMode' has the current
        // mode, so it's an index into the global volume array.
        //
        // And there are two local volumes as well: one for the default audio
        // device, another for secondary devices.  This allows tables levels
        // to be tweaked independently for each device.  Virtual pinball
        // systems often have two sound systems, one for the music/ROM sounds
        // and one for mechanical sound effects from the playfield.  Tables
        // can differ enough in the balance between the two types of effects
        // that it can be useful to control them separately.
        public float[] globalVolume = new float[2];
        public float defaultVolume = .66f;
        public bool globalMute = false;
        public float localVolume = 0.0f;
        public float local2Volume = 0.0f;
        public VolumeMode volumeMode = VolumeMode.Day;
        public VolumeModeSource volumeModeSource = VolumeModeSource.None;

        // Current volume mode
        public enum VolumeMode
        {
            Day = 0,
            Night = 1
        };

        // Volume mode source.  When we make a change to night mode, we'll note
        // where it came from.  Changes made via the keyboard interface override
        // changes made via Pinscape units.
        public enum VolumeModeSource { None, Keyboard, Pinscape };

        // control to KeyField mapper
        public Dictionary<Control, KeyField> fieldMap = new Dictionary<Control, KeyField>();

        public class KeyField
        {
            public KeyField(UIWin form, String name, Config.KeyInfo key, TextBox textBox, bool autoRepeat, Action handler)
            {
                // remember the name
                this.name = name;

                // remember the configuration entry
                this.key = key;

                // remember whether auto-repeat is allowed for the key
                this.autoRepeat = autoRepeat;

                // remember the controls
                this.textBox = textBox;

                // set up the click and Enter key handlers on the text field
                KeyField f = this;
                textBox.KeyPress += (object sender, KeyPressEventArgs ev) => { f.OnClick(form, ev); };
                textBox.MouseDown += (object sender, MouseEventArgs ev) => { f.OnClick(form, ev); };
                JoystickDev.JoystickButtonChanged += OnJoystickButton;

                // create our hot key
                hotkey = new Hotkey(key);
                hotkey.AutoRepeat = autoRepeat;
                hotkey.Pressed += (object sender, HandledEventArgs args) => 
                {
                    // skip if the key selection dialog is open
                    if (!keyDlgOpen)
                    {
                        handler();
                        args.Handled = true;
                    }
                };
                this.handler = handler;

                // add the control mapping
                form.fieldMap[textBox] = this;
            }

            // handle joystick button events
            public void OnJoystickButton(object sender, JoystickDev.JoystickEventArgs ev)
            {
                if (!keyDlgOpen                     // ignore if the key selector dialog is open
                    && (sender as JoystickDev).instanceGuid == key.jsGuid   // only process events on our assigned joystick
                    && ev.button == key.jsButton    // only process events on our assigned button
                    && ev.down                      // only process key down events
                    && (!ev.repeat || autoRepeat))  // only process first key down events, or auto repeats if enabled
                {
                    handler();
                }
            }

            // clean up
            public void Cleanup()
            {
                Unregister();
                JoystickDev.JoystickButtonChanged -= OnJoystickButton;
            }

            // register the system hotkey
            public void Register(Form form)
            {
                if (hotkey.KeyCode != Keys.None)
                {
                    try
                    {
                        if (!hotkey.Register(form))
                            Log.Error("Unable to set up hotkey for " + name + "(" + key.ToString(true) + "); "
                                + "the same key is already in use for another hotkey.");
                    }
                    catch (Exception exc)
                    {
                        Log.Error("Error setting up hotkey for " + name + "(" + key.ToString(true) + "): " + exc.Message);
                    }
                }
            }

            // unregister the system hotkey
            public void Unregister()
            {
                hotkey.Unregister();
            }

            // is the key registered?
            public bool Registered()
            {
                return hotkey.Registered;
            }

            // status report for logging
            public String Status()
            {
                return key.ToString() 
                    + (key.key == Keys.None ? "" : 
                       hotkey.Registered ? "" : 
                       " !!Hotkey registration failed");
            }

            // load the current configuration value into the UI controls
            public void ConfigToUI()
            {
                // set the controls
                textBox.Text = key.ToString(true);

                // set the key
                hotkey.Set(key);
            }

            // set flags
            public void SetShift(UIWin win, bool f)
            {
                if (key.shift != f)
                {
                    win.BeforeKeyChange();
                    key.SetShift(f);
                    ConfigToUI();
                }
            }

            public void SetCtrl(UIWin win, bool f)
            {
                if (key.control != f)
                {
                    win.BeforeKeyChange();
                    key.SetControl(f);
                    ConfigToUI();
                }
            }

            public void SetAlt(UIWin win, bool f)
            {
                if (key.alt != f)
                {
                    win.BeforeKeyChange();
                    key.SetAlt(f);
                    ConfigToUI();
                }
            }

            public void SetWindows(UIWin win, bool f)
            {
                if (key.windows != f)
                {
                    win.BeforeKeyChange();
                    key.SetWindows(f);
                    ConfigToUI();
                }
            }

            // handle a click in our UI text field
            public void OnClick(UIWin win, EventArgs ev)
            {
                MouseEventArgs mouse = null;
                KeyPressEventArgs key = null;
                bool go = false;
                if ((mouse = ev as MouseEventArgs) != null)
                {
                    // ask for a key on a right click only
                    go = mouse.Button == MouseButtons.Left;
                }
                else if ((key = ev as KeyPressEventArgs) != null)
                {
                    // ask for a key on a Return key only
                    go = key.KeyChar == '\r' || key.KeyChar == '\n';
                    if (go)
                        key.Handled = true;
                }

                if (go)
                {
                    // note that we're asking for a key
                    keyDlgOpen = true;

                    // show the dialog
                    GetKey dlg = new GetKey(this, key != null);
                    dlg.ShowDialog(win);

                    // done asking for a key
                    keyDlgOpen = false;

                    // if they selected a key, set the key
                    if (dlg.hasKey)
                        SetKey(win, dlg);
                }
            }

            // the key dialog is open
            public static bool keyDlgOpen = false;

            public void SetKey(UIWin win, GetKey dlg)
            {
                SetKey(win, dlg.key, dlg.shift, dlg.control, dlg.alt, dlg.windows, dlg.joystick, dlg.joystickButton);
            }

            public void SetKey(UIWin win, Keys key, bool shift = false, bool control = false, bool alt = false, bool windows = false)
            {
                SetKey(win, key, shift, control, alt, windows, Guid.Empty, -1);
            }

            public void SetKey(UIWin win, Guid jsGuid, int jsButton)
            {
                SetKey(win, Keys.None, false, false, false, false, jsGuid, jsButton);
            }

            public void SetKey(UIWin win, Keys key, bool shift, bool control, bool alt, bool windows, Guid jsGuid, int jsButton)
            {
                win.BeforeKeyChange();
                this.key.SetKey(key);
                this.key.SetShift(shift);
                this.key.SetControl(control);
                this.key.SetAlt(alt);
                this.key.SetWindows(windows);
                this.key.jsGuid = jsGuid;
                this.key.jsButton = jsButton;
                ConfigToUI();
            }

            String name;
            TextBox textBox;
            Config.KeyInfo key;
            bool autoRepeat;
            Hotkey hotkey;
            Action handler;
        }

        KeyField globalUpKey, globalDownKey;
        KeyField nightModeKey, globalMuteKey;
        KeyField localUpKey, localDownKey;
        KeyField local2UpKey, local2DownKey;

        // configuration data
        public Config cfg;

        // internal audio device descriptor
        public class AudioDevice
        {
            public String id;                       // system ID string, for accessing the device
            public String name;                     // friendly name, for display in the UI
            public bool isActive = false;           // this device is included in our control list
            public IAudioEndpointVolume epvol;      // volume control interface
            public float relLevel = 1.0f;           // volume level relative to the default output
            public bool useLocal = true;            // apply local volume level changes to this device
        }
        Dictionary<String, AudioDevice> audioDevices = new Dictionary<String, AudioDevice>();

        // default audio endpoint
        AudioDevice defaultDevice;

        // Build the audio device list from the devices currently present in the 
        // system, using the Windows device discovery mechanism.
        void GetAudioDevices()
        {
            // get the multimedia device enumerator
            IMMDeviceEnumerator mme = new MMDeviceEnumerator() as IMMDeviceEnumerator;

            // get the default audio endpoint
            IMMDevice ddev;
            mme.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole, out ddev);

            // get the default endpoint's ID
            String defId;
            ddev.GetId(out defId);

            // enumerate the active devices
            IMMDeviceCollection coll;
            mme.EnumAudioEndpoints(EDataFlow.eRender, DeviceState.Active, out coll);
            int devcnt;
            coll.GetCount(out devcnt);
            for (int i = 0; i < devcnt; ++i)
            {
                // create a table entry 
                AudioDevice ad = new AudioDevice();

                // get the device from the collection
                IMMDevice idev;
                coll.Item(i, out idev);

                // get the ID
                idev.GetId(out ad.id);

                // get the Friendly Name property
                IPropertyStore props;
                PropVariant nameProp;
                idev.OpenPropertyStore(StorageAccessMode.Read, out props);
                props.GetValue(ref FunctionDiscoveryKeys.FriendlyName, out nameProp);
                ad.name = nameProp.Value.ToString();

                // get the endpoint volume interface
                object obj;
                Guid mmdeGuid = typeof(IAudioEndpointVolume).GUID;
                idev.Activate(ref mmdeGuid, ClsCtx.INPROC_SERVER, IntPtr.Zero, out obj);
                ad.epvol = obj as IAudioEndpointVolume;

                // note if it's the default interface
                if (ad.id == defId)
                {
                    // remember it as the default
                    defaultDevice = ad;
                    
                    // the default device is always active
                    ad.isActive = true;

                    // get notifications of volume changes
                    ad.epvol.RegisterControlChangeNotify(this);
                }

                // add it to the table
                audioDevices[ad.id] = ad;
            }
        }

        // Pinscape controller list.  Pinscape units report their Night Mode status in
        // their ongoing joystick reports.  We'll sync our Night Mode setting with any
        // Pinscape unit we find.  (If ANY PS unit is in Night Mode, we'll consider the
        // whole system to be in night mode.)
        List<PinscapeDev> pinscapeUnits;
        Thread pinscapeThread = null;
        EventWaitHandle pinscapeThreadExit = null;
        void PinscapeMonitor()
        {
            for (;;)
            {
                // determine if any Pinscape units are in night mode
                bool night = pinscapeUnits.Any(p => p.InNightMode());

                // If this is a change from our current status, set the new status.
                // Make the update via Invoke so that it's handled on the main UI
                // thread, to avoid any cross-thread unpleasantness.  Note that we
                // could still have a race condition with Night Mode changes made
                // via the keyboard UI in the other thread, but 
                if (night != (volumeMode == VolumeMode.Night))
                {
                    Invoke((Action)(delegate {
                        SetNightMode(VolumeModeSource.Pinscape, night, OSDWin.OSDType.Global);
                    }));
                }

                // wait a couple of seconds before the next round, or until the
                // thread exit event is signaled
                if (pinscapeThreadExit.WaitOne(2000))
                    break;
            }
        }

        internal static class UsbNotification
        {
            static public void Register(IntPtr hwnd)
            {
                // set up the device descriptor for USB devices
                DevBroadcastDeviceinterface dbi = new DevBroadcastDeviceinterface
                {
                    DeviceType = DbtDevtypDeviceinterface,
                    Reserved = 0,
                    ClassGuid = GuidDevinterfaceUSBDevice,
                    Name = 0
                };

                // marshal it to a byte buffer
                dbi.Size = Marshal.SizeOf(dbi);
                IntPtr buffer = Marshal.AllocHGlobal(dbi.Size);
                Marshal.StructureToPtr(dbi, buffer, true);

                // register
                notificationHandle = RegisterDeviceNotification(
                    hwnd, buffer, DEVICE_NOTIFY_WINDOW_HANDLE);

                // done with the byte buffer
                Marshal.FreeHGlobal(buffer);
            }

            static public void Unregister()
            {
                UnregisterDeviceNotification(notificationHandle);
            }

            [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal extern static IntPtr RegisterDeviceNotification(
                IntPtr hRecipient, IntPtr notificationFilter, int flags);

            [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal extern static bool UnregisterDeviceNotification(
                IntPtr handle);

            [StructLayout(LayoutKind.Sequential)]
            private struct DevBroadcastDeviceinterface
            {
                internal int Size;
                internal int DeviceType;
                internal int Reserved;
                internal Guid ClassGuid;
                internal short Name;
            }

            public const int DbtDevicearrival = 0x8000; // system detected a new device        
            public const int DbtDeviceremovecomplete = 0x8004; // device is gone      
            public const int WmDevicechange = 0x0219; // device change event      
            public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
            public const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001;
            public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004;
            private const int DbtDevtypDeviceinterface = 5;
            private static readonly Guid GuidDevinterfaceUSBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices
            private static IntPtr notificationHandle;

        }

        // Start the UI
        private void Form1_Load(object sender, EventArgs e)
        {
            // Set the build number tool tip
            String version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            tipVersion.SetToolTip(picLogo, "PinVol version " + version);
            lblVersion.Text = "v." + version;

            // Look for attached Pincsape controllers.  If we find any, start a thread
            // to monitor it/them for Night Mode status changes.
            pinscapeUnits = PinscapeDev.FindDevices();
            if (pinscapeUnits.Count != 0)
            {
                pinscapeThread = new Thread(PinscapeMonitor);
                pinscapeThreadExit = new EventWaitHandle(false, EventResetMode.ManualReset);
                pinscapeThread.Start();
            }

            // load the configuration
            cfg = new Config(this);
            cfg.Load();

            // if any keys are mapped to joystick buttons, enable the joystick even if
            // it's disabled in the configuration
            if (!cfg.EnableJoystick && cfg.keys.Any(kv => kv.Value.jsGuid != Guid.Empty))
            {
                // turn on the joystick after all
                cfg.EnableJoystick = true;
                SetCfgDirty();
            }

            // enable joystick handling if desired
            EnableJoysticks(cfg.EnableJoystick);
            ckEnableJoystick.Checked = cfg.EnableJoystick;

            // enable independent per-table volume settings for secondary audio devices if desired
            EnableLocal2(cfg.EnableLocal2);
            ckEnableLocal2.Checked = cfg.EnableLocal2;

            // register for device notification broadcast messages from the system,
            // so that we can detect a new joystick being plugged in
            UsbNotification.Register(this.Handle);

            // get the audio device list
            GetAudioDevices();

            // restore audio device configuration settings
            cfg.RestoreAudioDevices(audioDevices);

            // if the settings were hidden last time, hide them again
            if (!cfg.ShowSettings)
                HideSettings();

            // set the initial external volume change setting
            ckExtIsLocal.Checked = cfg.ExtVolIsLocal;

            // set the initial unmute-on-volume-change setting
            ckUnmuteOnVolChange.Checked = cfg.UnMuteOnVolChange;

            // set the initial OSD checkboxes
            ckOSDOnHotkeys.Checked = cfg.OSDOnHotkeys;
            ckOSDOnAppSwitch.Checked = cfg.OSDOnAppSwitch;

            // assume we're starting in day mode
            volumeMode = 0;
            picNightMode.Visible = false;

            // set invalid initial global volume levels, so that we can detect if
            // we fail to load saved volume levels
            globalVolume[0] = globalVolume[1] = defaultVolume = -1.0f;

            // Load the saved global volume level and local volume database.  This
            // will replace the defaults we just set (based on the current device
            // volume settings in the system) if we have saved global levels.
            LoadSavedVols();

            // start with the local volume for "System"
            LocalVol v = appVol.ContainsKey(AppMonitor.GlobalContextName) ? 
                appVol[AppMonitor.GlobalContextName] : new LocalVol(1.0f, 1.0f);

            // pull out the primary and secondary local volumes, limiting the valid levels
            localVolume = LimitVolume(v.primary);
            local2Volume = LimitVolume(v.secondary);

            // If we didn't read valid saved global volumes, apply defaults.
            if (globalVolume[0] <= 0)
            {
                // We didn't load a saved day mode volume.  Use the current system 
                // volume setting for the default device as the default.  Set the 
                // global volume such that the product of the global and local 
                // volumes equals the system volume.
                float sysvol;
                defaultDevice.epvol.GetMute(out globalMute);
                defaultDevice.epvol.GetMasterVolumeLevelScalar(out sysvol);
                globalVolume[0] = LimitVolume(defaultDevice.useLocal ? sysvol / localVolume : sysvol);
            }
            if (globalVolume[1] <= 0)
            {
                // We didn't load a saved night mode volume.  Use half the day-mode 
                // level by default.
                globalVolume[1] = LimitVolume(globalVolume[0] / 2);
            }
            if (defaultVolume < 0)
                defaultVolume = 0.66f;
           
            // update the volume controls to match
            UpdateVolume(OSDWin.OSDType.None);

            // update the default volume control
            int nominalDefaultVolume = (int)Math.Round(defaultVolume * 100.0f);
            lblDefaultVol.Text = nominalDefaultVolume + "%";
            trkDefaultVol.Value = nominalDefaultVolume;

            // wire the UI controls for the keys to the config elements and hotkeys
            globalUpKey = new KeyField(this, "Global Volume Up", cfg.keys["globalVolUp"], txtGlobalUp,  true,
                () => { GlobalVolumeAdjust(.01f, cfg.OSDOnHotkeys ? OSDWin.OSDType.Global : OSDWin.OSDType.None); });
            globalDownKey = new KeyField(this, "Global Volume Down", cfg.keys["globalVolDown"], txtGlobalDown, true,
                () => { GlobalVolumeAdjust(-.01f, cfg.OSDOnHotkeys ? OSDWin.OSDType.Global : OSDWin.OSDType.None); });
            nightModeKey = new KeyField(this, "Night Mode", cfg.keys["nightMode"], txtNightMode, false,
                () => { ToggleNightMode(VolumeModeSource.Keyboard, cfg.OSDOnHotkeys ? OSDWin.OSDType.Global : OSDWin.OSDType.None); });
            globalMuteKey = new KeyField(this, "Mute", cfg.keys["mute"], txtMute, false,
                () => { ToggleMute(cfg.OSDOnHotkeys ? osdType : OSDWin.OSDType.None); });
            localUpKey = new KeyField(this, "Table Volume Up", cfg.keys["localVolUp"], txtLocalUp, true,
                () => { LocalVolumeAdjust(.01f, cfg.OSDOnHotkeys ? OSDWin.OSDType.Local : OSDWin.OSDType.None); });
            localDownKey = new KeyField(this, "Table Volume Down", cfg.keys["localVolDown"], txtLocalDown, true,
                () => { LocalVolumeAdjust(-.01f, cfg.OSDOnHotkeys ? OSDWin.OSDType.Local : OSDWin.OSDType.None); });
            local2UpKey = new KeyField(this, "Table Volume Up (Secondary)", cfg.keys["local2VolUp"], txtLocal2Up, true,
                () => { Local2VolumeAdjust(.01f, cfg.OSDOnHotkeys ? OSDWin.OSDType.Local2 : OSDWin.OSDType.None); });
            local2DownKey = new KeyField(this, "Table Volume Down (Secondary)", cfg.keys["local2VolDown"], txtLocal2Down, true,
                () => { Local2VolumeAdjust(-.01f, cfg.OSDOnHotkeys ? OSDWin.OSDType.Local2 : OSDWin.OSDType.None); });

            // set the initial UI state to the loaded config values
            AllKeyConfigToUI();

            // register all keys
            RegisterKeys();

            // log startup status
            LogKeyStatus("Hotkeys assigned:");

            // no OSD window yet (we create it as needed)
            osdwin = null;
        }

        // enable/disable independent per-table settings for secondary audio devices
        void EnableLocal2(bool enable)
        {
            // register or unregister the hotkeys for the secondary local controls
            if (local2UpKey != null && local2DownKey != null)
            {
                if (enable)
                {
                    local2UpKey.Register(this);
                    local2DownKey.Register(this);
                }
                else
                {
                    local2UpKey.Unregister();
                    local2DownKey.Unregister();
                }
            }

            // enable or disable the hotkey text fields
            txtLocal2Up.Enabled = enable;
            txtLocal2Down.Enabled = enable;
        }

        // enable/disable joystick input
        void EnableJoysticks(bool enable)
        {
            if (enable)
            {
                // enable the joystick subsystem - this enumerates attached
                // devices and sets up event handling for buttons
                JoystickDev.Enable(this);

                // if this is the first time through, log the list of devices
                // we discovered
                if (!joysticksEverEnabled)
                {
                    joysticksEverEnabled = true;
                    JoystickDev.LogDeviceList();
                }
            }
            else
            {
                // disable the joystick subsystem
                JoystickDev.Disable();
            }
        }
        bool joysticksEverEnabled = false;

        // Handle a joystick device error
        public void OnJoystickError(JoystickDev js)
        {
            // if any buttons are mapped to this joystick, log an error
            bool logIt = cfg.keys.Any(k => k.Value.jsGuid == js.instanceGuid);
            if (logIt)
                Log.Error("Error reading status for joystick " + js.unitNo + " (" + js.UnitName + ")");
            
            // if joysticks are still enabled, rebuild the joystick list
            if (cfg.EnableJoystick)
            {
                // power-cycle the joystick subsystem to build the new list
                JoystickDev.Disable();
                JoystickDev.Enable(this);

                // update the UI to reflect any changes to joystick unit numbers
                AllKeyConfigToUI();
            }
        }

        // Application message loop filter
        protected override void WndProc(ref Message m)
        {
            // call the base class handler
            base.WndProc(ref m);

            // check for Device Change notifications
            if (m.Msg == UsbNotification.WmDevicechange)
            {
                // check the subtype
                if ((int)m.WParam == UsbNotification.DbtDevicearrival)
                {
                    // Device arrival - a new device was plugged in.  If joystick
                    // input is enabled, set a timer to scan for new joysticks.
                    // Do this on a timer, since DirectInput doesn't always seem
                    // to recognize new joysticks immediately when this message
                    // arrives.
                    if (cfg.EnableJoystick)
                    {
                        joystickRescanCnt++;
                        joystickRescanTimer.Enabled = true;
                    }
                }
            }
        }

        // deferred joystick re-scan
        int joystickRescanCnt = 0;
        private void joystickRescanTimer_Tick(object sender, EventArgs e)
        {
            // check for new joysticks
            JoystickDev.Rescan();

            // update the UI to reflect any changes to joystick unit numbers
            AllKeyConfigToUI();

            // if this is the last pending rescan, disable the timer
            if (--joystickRescanCnt <= 0)
            {
                joystickRescanCnt = 0;
                joystickRescanTimer.Enabled = false;
            }
        }


        // update the UI to reflect the current key assignments
        void AllKeyConfigToUI()
        {
            globalUpKey.ConfigToUI();
            globalDownKey.ConfigToUI();
            nightModeKey.ConfigToUI();
            globalMuteKey.ConfigToUI();
            localUpKey.ConfigToUI();
            localDownKey.ConfigToUI();
            local2UpKey.ConfigToUI();
            local2DownKey.ConfigToUI();
        }

        private void LogKeyStatus(String header)
        {
            Log.Info(header);
            Log.Info("  Global volume up: " + globalUpKey.Status());
            Log.Info("  Global volume down: " + globalDownKey.Status());
            Log.Info("  Night mode toggle:  " + nightModeKey.Status());
            Log.Info("  Mute toggle: " + globalMuteKey.Status());
            Log.Info("  Table volume up: " + localUpKey.Status());
            Log.Info("  Table volume down: " + localDownKey.Status());
            Log.Info("  Table volume up 2nd device: " + local2UpKey.Status());
            Log.Info("  Table volume down 2nd device: " + local2DownKey.Status());
        }

        // our private GUID for making system volume level changes, so that we can
        // recognize our own changes when responding to change events
        public Guid OurEventGuid = new Guid("9c7a3804-15d5-4950-ae70-ad5d99da7664");

        // system volume level change event handler - this is called whenever the
        // volume is changed by any program
        void IAudioEndpointVolumeCallback.OnNotify(ref AUDIO_VOLUME_NOTIFICATION_DATA data)
        {
            // Only respond to the change if it came from elsewhere.  If the event is
            // stamped with our GUID, we initiated it, so there's no need to respond
            // (and doing would be bad, since it could get us into looping trouble).
            if (new Guid(data.guidEventContext) != OurEventGuid)
            {
                // Handle the change in an Invoke callback to make sure it's
                // on the UI thread.  This is necessary because we want to
                // update the trackbar controls, which can only be done on the
                // UI thread, but it also has the pleasant side benefit of
                // avoiding the need for multithread locks to protect access 
                // to our shared variables.
                float vol = data.fMasterVolume;
                bool mute = data.bMuted;
                BeginInvoke((MethodInvoker)delegate { this.ExtSetVolume(vol, mute); });
            }
        }

        private void ToggleMute(OSDWin.OSDType osdType)
        {
            globalMute = !globalMute;
            UpdateVolume(osdType);
        }

        private void SetNightMode(VolumeModeSource source, bool engage, OSDWin.OSDType osdType)
        {
            // If the change is coming from Pinscape, but we previously set the
            // mode via a keystroke, ignore Pinscape.
            if (source == VolumeModeSource.Pinscape && volumeModeSource == VolumeModeSource.Keyboard)
                return;

            // get the new volume mode
            VolumeMode newVolumeMode = engage ? VolumeMode.Night : VolumeMode.Day;

            // if this represents a change, make the update
            if (newVolumeMode != volumeMode)
            {
                // set the new mode
                volumeMode = newVolumeMode;

                // note the new source
                volumeModeSource = source;

                // update the mode icon in the UI window
                picNightMode.Visible = (volumeMode == VolumeMode.Night);

                // update the system volume level for the change
                UpdateVolume(osdType);
            }
        }

        private void ToggleNightMode(VolumeModeSource source, OSDWin.OSDType osdType)
        {
            // if we're in day mode (volumeMode 0), engage night mode, otherwise disengage
            SetNightMode(source, volumeMode == 0, osdType);
        }

        private void GlobalVolumeAdjust(float delta, OSDWin.OSDType osdType)
        {
            SetGlobalVol(globalVolume[(int)volumeMode] + delta);
            CheckMute();
            UpdateVolume(osdType);
        }

        private void LocalVolumeAdjust(float delta, OSDWin.OSDType osdType)
        {
            SetLocalVol(localVolume + delta, local2Volume);
            CheckMute();
            UpdateVolume(osdType);
        }

        private void Local2VolumeAdjust(float delta, OSDWin.OSDType osdType)
        {
            SetLocalVol(localVolume, local2Volume + delta);
            CheckMute();
            UpdateVolume(osdType);
        }

        // Check muting after a change to a volume level.  If the config option
        // for "unmute on volume change" is on, we'll remove muting.
        void CheckMute()
        {
            if (cfg.UnMuteOnVolChange && globalMute)
                globalMute = false;
        }

        // Limit a volume level to the allowable range, .01 to 1.0.  Note that we
        // never allow our internal volume levels to drop to 0, because we might
        // have to divide by an internal level to determine the other internal
        // level after an external volume change.  See ExtSetVolume().
        private float LimitVolume(float vol, float low = 0.01f, float high = 1.0f)
        {
            return vol < low ? low : vol > high ? high : vol;
        }

        // Set the volume from an external source
        private void ExtSetVolume(float sysvol, bool mute)
        {
            // note the new mute setting
            globalMute = mute;            

            // Figure the new internal volume levels.  The given level 'newvol'
            // is the new system volume, so we have to figure out how this affects
            // our internal volume levels, which are split into the 'global' and
            // 'local' components.  This depends on the configuration setting
            // that tells us whether to apply external changes to the global or
            // local level.  In either case, we're simply solving for the affected
            // variable in our system volume formula:
            //
            //   system volume = global volume * local volume
            //
            if (cfg.ExtVolIsLocal)
            {
                // figure the new local volume that yields the new system volume
                // for the current global volume
                float newLocalVol = sysvol / globalVolume[(int)volumeMode];

                // set the new volume
                SetLocalVol(newLocalVol, local2Volume);
            }
            else
            {
                SetGlobalVol(sysvol / localVolume);
            }

            // Update the volume controls
            UpdateVolume(OSDWin.OSDType.None);
        }

        // Update the trackbar controls for a change in our volume levels.  Also
        // trigger the OSD volume display if desired.
        private void UpdateVolume(OSDWin.OSDType osdType)
        {
            // update the volume level for each active device in our list
            foreach (KeyValuePair<String, AudioDevice> kv in audioDevices)
            {
                // if the device is active, update its volume
                AudioDevice ad = kv.Value;
                if (ad.isActive)
                {
                    // start with the global volume, adjusted by the device's relative level
                    float vol = ad.relLevel * globalVolume[(int)volumeMode];

                    // If the device uses the local app volume, apply the appropriate
                    // local volume.
                    if (ad.useLocal)
                    { 
                        // Figure out which volume to use.  If this is a secondary device
                        // (not the default device), and we're using the independent local
                        // volume control for secondary devices, use the secondary volume.
                        // Otherwise use the main local volume.
                        float lcl = (ad != defaultDevice && cfg.EnableLocal2) ? 
                            local2Volume : localVolume;

                        // apply it
                        vol *= lcl;
                    }

                    // update the volume in the endpoint
                    ad.epvol.SetMasterVolumeLevelScalar(LimitVolume(vol), ref OurEventGuid);
                    ad.epvol.SetMute(globalMute, ref OurEventGuid);
                }
            }

            // update the local volume trackbar control
            int l = (int)Math.Round(localVolume * 100.0f);
            trkLocalVol.Value = l;
            lblLocalVol.Text = l + "%";

            // update the global volume trackbar control
            int g = (int)Math.Round(globalVolume[(int)volumeMode] * 100.0f);
            trkGlobalVol.Value = g;
            lblGlobalVol.Text = g + "%";

            // show and/or refresh the OSD window
            ShowOSD(osdType, osdHotkeyTime);
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal extern static bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_SHOW_NO_ACTIVATE = 4;

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal extern static bool SetWindowPos(IntPtr hWnd, IntPtr hwndInsertAfter, int X, int Y, int cx, int cy, UInt32 flags);
        static IntPtr HWND_BOTTOM = new IntPtr(1);
        static IntPtr HWND_NOTOPOST = new IntPtr(-2);
        static IntPtr HWND_TOP = new IntPtr(0);
        static IntPtr HWND_TOPMOST = new IntPtr(-1);
        const UInt32 SWP_ASYNCWINDOWPOS = 0x4000;
        const UInt32 SWP_DEFERERASE = 0x2000;
        const UInt32 SWP_DRAWFRAME = 0x0020;
        const UInt32 SWP_FRAMECHANGED = 0x0020;
        const UInt32 SWP_HIDEWINDOW = 0x0080;
        const UInt32 SWP_NOACTIVATE = 0x0010;
        const UInt32 SWP_NOCOPYBITS = 0x0100;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOOWNERZORDER = 0x0200;
        const UInt32 SWP_NOREDRAW = 0x0008;
        const UInt32 SWP_NOREPOSITION = 0x0200;
        const UInt32 SWP_NOSENDCHANGING = 0x0400;
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOZORDER = 0x0004;
        const UInt32 SWP_SWHOWINDOW = 0x0040;


        private void ShowOSD(OSDWin.OSDType newOsdType, int timeInMs)
        {
            // if there's no OSD window currently displayed, and a volume type
            // was specified, create the window
            if (osdwin == null && newOsdType != OSDWin.OSDType.None)
            {
                osdwin = new OSDWin(this);
                osdwin.Opacity = 0;
                osdwin.Visible = false;
                osdwin.Show();
            }

            // switch the OSD volume type if one was specified
            if (newOsdType != OSDWin.OSDType.None)
                osdType = newOsdType;

            // if there's an OSD window, update it
            if (osdwin != null)
            {
                // refresh the OSD window
                osdwin.Invalidate();

                // If desired, show it and start/restart the timer
                if (newOsdType != OSDWin.OSDType.None && !osdwin.InSetup())
                {
                    ShowWindow(osdwin.Handle, SW_SHOW_NO_ACTIVATE);
                    SetWindowPos(osdwin.Handle, HWND_TOPMOST, -1, -1, -1, -1, SWP_NOMOVE | SWP_NOSIZE | SWP_NOOWNERZORDER | SWP_NOACTIVATE);
                    osdwin.Opacity = 1.0f;

                    SetTimer(ref osdOffTime, timeInMs);
                    updateTimer.Enabled = true;
                }
            }
        }


        private void trkDefaultVol_Scroll(object sender, EventArgs e)
        {
            lblDefaultVol.Text = trkDefaultVol.Value + "%";
            defaultVolume = trkDefaultVol.Value / 100.0f;
            SetGlobalVolDirty();
        }

        private void trkGlobalVol_Scroll(object sender, EventArgs e)
        {
            SetGlobalVol(trkGlobalVol.Value / 100.0f);
            CheckMute();
            UpdateVolume(OSDWin.OSDType.None);
        }

        private void trkLocalVol_Scroll(object sender, EventArgs e)
        {
            SetLocalVol(trkLocalVol.Value / 100.0f, local2Volume);
            CheckMute();
            UpdateVolume(OSDWin.OSDType.None);
        }

        // register all keys
        private void RegisterKeys()
        {
            localUpKey.Register(this);
            localDownKey.Register(this);
            globalUpKey.Register(this);
            globalDownKey.Register(this);
            globalMuteKey.Register(this);
            nightModeKey.Register(this);
            if (cfg.EnableLocal2)
            {
                local2UpKey.Register(this);
                local2DownKey.Register(this);
            }
        }

        // unregister all keys
        private void UnregisterKeys()
        {
            localUpKey.Unregister();
            localDownKey.Unregister();
            globalUpKey.Unregister();
            globalDownKey.Unregister();
            globalMuteKey.Unregister();
            nightModeKey.Unregister();
            if (cfg.EnableLocal2)
            {
                local2UpKey.Unregister();
                local2DownKey.Unregister();
            }
        }

        // On making a change to any key assignments, we'll un-register ALL of the
        // hotkeys and set a timer to re-register everything in a couple of seconds.
        // We do all of the keys as a group because otherwise we could have
        // registration failures due to conflicts between two keys that are in
        // a partially updated state.
        private void BeforeKeyChange()
        {
            // unregister all hotkeys
            UnregisterKeys();

            // note that we have to re-register keys and save the new configuration
            SetRegDirty();
            SetCfgDirty();
        }

        // We've made a change to the key assignments, necessitating re-registering 
        // the hotkeys.  regTime is the time to re-register.
        private bool regDirty = false;
        private DateTime regTime = DateTime.Now;
        public void SetRegDirty() 
        { 
            regDirty = true; 
            SetTimer(ref regTime, 1000);
        }

        // We have unsaved configuration changes.
        private bool cfgDirty = false;
        private DateTime cfgTime = DateTime.Now;
        public void SetCfgDirty() 
        {
            cfgDirty = true;
            SetTimer(ref cfgTime, 5000);
        }

        // We have unsaved local volume level changes
        private bool localVolDirty = false;
        private DateTime localVolTime = DateTime.Now;
        public void SetLocalVolDirty()
        {
            localVolDirty = true;
            SetTimer(ref localVolTime, 5000);
        }

        // We have unsaved global volume level changes
        private bool globalVolDirty = false;
        private DateTime globalVolTime = DateTime.Now;
        public void SetGlobalVolDirty()
        {
            globalVolDirty = true;
            SetTimer(ref globalVolTime, 5000);
        }

        // update a delay timer
        void SetTimer(ref DateTime timerVar, int delayInMs)
        {
            // figure the new off time
            DateTime t = DateTime.Now.AddMilliseconds(delayInMs);

            // if the current delay isn't already past this point, set the new time
            if (timerVar < t)
                timerVar = t;

            // enable the update timer
            updateTimer.Enabled = true;
        }

        // set the dirty flags
        public void BeginFadeOSD()
        {
            // set the OSD time so that it looks like we're at the start of the fade period
            osdOffTime = DateTime.Now;
            updateTimer.Enabled = true;
        }

        // work intervals (milliseconds)
        int osdHotkeyTime = 2000;       // time to display the OSD after a volume hotkey press
        int osdAppSwitchTime = 4000;    // time to display the OSD after an application switch

        // check for pending work
        private void updateTimer_Tick(object sender, EventArgs e)
        {
            // re-register hotkeys if necessary
            DateTime now = DateTime.Now;
            if (regDirty && now >= regTime)
            {
                // re-register all keys
                RegisterKeys();
                LogKeyStatus("Hotkeys changed:");
                regDirty = false;
            }

            // save the configuration if we have unsaved changes
            if (cfgDirty && now >= cfgTime)
            {
                cfg.Save();
                cfgDirty = false;
            }

            // fade the volume window, if present
            if (osdwin != null && !osdwin.InSetup() && now >= osdOffTime)
            {
                // fade out by a 10% step; if that takes us to fully transparent, make
                // the window explicitly invisible, which will stop the timed process
                osdwin.Opacity -= .1;
                if (osdwin.Opacity == 0)
                {
                    osdwin.Close();
                    osdwin = null;
                }
            }

            // flush the volume database if we have unsaved changes
            if (localVolDirty && now >= localVolTime)
            {
                SaveLocalVols();
                localVolDirty = false;
            }

            // flush the global volume file if we have unsaved changes
            if (globalVolDirty && now >= globalVolTime)
            {
                SaveGlobalVols();
                globalVolDirty = false;
            }

            // disable the timer if there's no more pending work
            if (!(cfgDirty || regDirty || localVolDirty || globalVolDirty || (osdwin != null && !osdwin.InSetup())))
                updateTimer.Enabled = false;
        }

        private void UIWin_FormClosed(object sender, FormClosedEventArgs e)
        {
            // close out the configuration if we have unsaved changes
            if (cfgDirty && cfg != null)
            {
                cfgDirty = false;
                cfg.Save();
            }

            // close out the local volume level database if we have unsaved changed
            if (localVolDirty)
            {
                localVolDirty = false;
                SaveLocalVols();
            }

            // close out the global volume level settings file
            if (globalVolDirty)
            {
                globalVolDirty = false;
                SaveGlobalVols();
            }

            // shut down the Pinscape monitor thread
            if (pinscapeThread != null)
            {
                pinscapeThreadExit.Set();
                pinscapeThread.Join(2500);
            }

            // unregister the epvol callback
            if (defaultDevice != null && defaultDevice.epvol != null)
                defaultDevice.epvol.UnregisterControlChangeNotify(this);

            // unregister our USB device notification
            UsbNotification.Unregister();

            // clean up our keys
            if (globalUpKey != null) globalUpKey.Cleanup();
            if (globalDownKey != null) globalDownKey.Cleanup();
            if (globalMuteKey != null) globalMuteKey.Cleanup();
            if (localUpKey != null) localUpKey.Cleanup();
            if (localDownKey != null) localDownKey.Cleanup();
            if (local2UpKey != null) local2UpKey.Cleanup();
            if (local2DownKey != null) local2DownKey.Cleanup();
            if (nightModeKey != null) nightModeKey.Cleanup();
        }

        private void btnHideSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // hide the settings area visually
            HideSettings();

            // update the config to hide the settings next time we start up
            cfg.ShowSettings = false;
            SetCfgDirty();
        }

        private Size origWinSize;
        private void HideSettings()
        {
            // remember the original window size
            origWinSize = this.ClientSize;

            // resize the window to hide everything below the top of the settings panel
            this.ClientSize = new Size(origWinSize.Width, settingsPanel.Top - 1);
            btnShowSettings.Visible = true;
            settingsPanel.Visible = false;
        }

        private void btnShowSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // restore the original window size
            this.ClientSize = origWinSize;
            btnShowSettings.Visible = false;
            settingsPanel.Visible = true;

            // update the config to show the settings
            cfg.ShowSettings = true;
            SetCfgDirty();
        }

        private void ckExtIsLocal_CheckedChanged(object sender, EventArgs e)
        {
            bool f = ckExtIsLocal.Checked;
            if (cfg.ExtVolIsLocal != f)
            {
                cfg.ExtVolIsLocal = f;
                SetCfgDirty();
            }
        }

        private void ckOSDOnHotkeys_CheckedChanged(object sender, EventArgs e)
        {
            bool f = ckOSDOnHotkeys.Checked;
            if (cfg.OSDOnHotkeys != f)
            {
                cfg.OSDOnHotkeys = f;
                SetCfgDirty();
            }
        }

        private void ckOSDOnAppSwitch_CheckedChanged(object sender, EventArgs e)
        {
            bool f = ckOSDOnAppSwitch.Checked;
            if (cfg.OSDOnAppSwitch != f)
            {
                cfg.OSDOnAppSwitch = f;
                SetCfgDirty();
            }
        }

        private void ckUnmuteOnVolChange_CheckedChanged(object sender, EventArgs e)
        {
            bool f = ckUnmuteOnVolChange.Checked;
            if (cfg.UnMuteOnVolChange != f)
            {
                cfg.UnMuteOnVolChange = f;
                SetCfgDirty();
            }
        }

        private void ckEnableJoystick_CheckedChanged(object sender, EventArgs e)
        {
            // check for a change in status
            bool f = ckEnableJoystick.Checked;
            if (cfg.EnableJoystick != f)
            {
                // save the config update
                cfg.EnableJoystick = f;
                SetCfgDirty();

                // enable or disable joysticks
                EnableJoysticks(f);
            }
        }

        private void ckEnableLocal2_CheckedChanged(object sender, EventArgs e)
        {
            // check for a change in status
            bool f = ckEnableLocal2.Checked;
            if (cfg.EnableLocal2 != f)
            {
                // save the config update
                cfg.EnableLocal2 = f;
                SetCfgDirty();

                // enable or disable secondary volume control
                EnableLocal2(f);
            }
        }


        private void btnSetUpOSD_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (osdwin == null)
            {
                ShowOSD(OSDWin.OSDType.Local, 0);
                osdwin.Setup(true);
            }
        }

        private void keyMenuNoKey_Click(object sender, EventArgs e)
        {
            Control txtbox = ContextMenuSource(sender);
            if (txtbox != null && fieldMap[txtbox] != null)
                fieldMap[txtbox].SetKey(this, Keys.None);
        }

        Control ContextMenuSource(object sender)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                ContextMenuStrip strip = item.Owner as ContextMenuStrip;
                if (strip != null)
                    return strip.SourceControl;
            }
            return null;
        }

        private void appCheckTimer_Tick(object sender, EventArgs e)
        {
            // remember the old application type
            var oldAppType = appmon.GetAppType();

            // check for changes in the foreground application
            if (appmon.CheckActiveApp())
            {
                // set the label showing the current app
                lblCurApp.Text = appmon.FriendlyName;

                // remember the new app
                curApp = appmon.App;

                // If there's a saved level for this app, apply it.  Otherwise, set
                // the default local volume.
                if (appVol.ContainsKey(appmon.App))
                    SetLocalVol(appVol[appmon.App]);
                else
                    SetLocalVol(defaultVolume, defaultVolume);

                UpdateVolume(OSDWin.OSDType.None);

                // If desired, bring up the OSD.  Skip this if the application type
                // isn't changing - e.g., if we're switching between games in PinballY.
                if (cfg.OSDOnAppSwitch && appmon.GetAppType() != oldAppType)
                    ShowOSD(osdType, osdAppSwitchTime);
            }

            // check for new errors
            if (Log.nErrors != 0)
            {
                lblErrorAlert.Visible = true;
                btnViewErrors.Visible = true;
            }
        }

        private void btnViewErrors_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Log.ViewLog();
        }

        private void lnkSelectDevices_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // run the device selector dialog
            AudioOutputs ad = new AudioOutputs(audioDevices.Values.ToArray(), defaultDevice);
            ad.ShowDialog(this);

            // update the configuration with any changes
            bool changed = false;
            foreach (var p in audioDevices)
            {
                AudioDevice dev = p.Value;
                changed |= cfg.UpdateDevice(dev.id, dev.name, dev.isActive, dev.relLevel, dev.useLocal);
            }

            // if we made any changes, mark the config as dirty
            if (changed)
                SetCfgDirty();
        }

        private void picLogo_Click(object sender, EventArgs e)
        {
            lblVersion.Visible = !lblVersion.Visible;
        }

        private void configToolCheckTimer_Tick(object sender, EventArgs e)
        {
            // check for recent Config Tool notification timeouts
            Program.configToolTimeout();
        }

    }
}
