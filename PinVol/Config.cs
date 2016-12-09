using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Drawing;

namespace PinVol
{
    public class Config
    {
        public Config()
        {
            // set up default key assignments
            keys["globalVolUp"] = new KeyInfo(Keys.F10, false, false, false, true);
            keys["globalVolDown"] = new KeyInfo(Keys.F9, false, false, false, true);
            keys["mute"] = new KeyInfo(Keys.F8, false, false, false, true);
            keys["localVolUp"] = new KeyInfo(Keys.VolumeUp);
            keys["localVolDown"] = new KeyInfo(Keys.VolumeDown);
            keys["nightMode"] = new KeyInfo(Keys.F7, false, false, false, true);
        }

        // Miscellaneous configuration settings
        public bool ShowSettings = true;                // settings are visible
        public bool ExtVolIsLocal = false;              // external volume changes are applied to the local volume
        public bool OSDOnHotkeys = true;                // show the on-screen volume display on hotkey volume changes
        public bool OSDOnAppSwitch = false;             // show the on-screen volume display on application switching
        public int OSDRotation = -90;                   // rotation matrix for on-screen volume display
        public bool UnMuteOnVolChange = true;           // turn off muting whenever the volume is changed
        public Rectangle OSDPos = new Rectangle(50, 50, 680, 180);    // overlay position
        public bool EnableJoystick = false;             // enable joystick input

        // Audio device record.  This represents the saved config data
        // corresponding to a UIWin.AudioDevice object.  
        //
        // We identify devices by their IMMDevice ID strings.  This ID
        // is specifically designed in the Windows audio architecture
        // for identifying the device across sessions.
        //
        // The configuration only records devices that are active (i.e.,
        // included in the group we're controlling).  That's why there's
        // no "isActive" flag here.
        public class CfgAudioDevice
        {
            public CfgAudioDevice(String id, String name, int relVol, bool useLocal)
            {
                this.id = id;
                this.name = name;
                this.relVol = relVol;
                this.useLocal = useLocal;
            }

            // update the entry; returns true if any changes were made
            public bool Update(String id, String name, int relVol, bool useLocal)
            {
                if (this.id != id 
                    || this.name != name 
                    || this.relVol != relVol 
                    || this.useLocal != useLocal)
                {
                    this.id = id;
                    this.name = name;
                    this.relVol = relVol;
                    this.useLocal = useLocal;
                    return true;
                }
                else
                    return false;
            }

            public String id;       // IMMDevice ID string
            public String name;     // device name, for documentary purposes only
            public int relVol;      // relative volume level setting, as a percentage value
            public bool useLocal;   // use the local (per-table) volume adjustment with this device
        }

        // list of saved audio devices
        Dictionary<String, CfgAudioDevice> devices = new Dictionary<String, CfgAudioDevice>();

        // Update a saved device with new settings.  Returns true if any
        // changes were made.
        public bool UpdateDevice(String id, String name, bool isActive, float fRelVol, bool useLocal)
        {
            // convert the relative volume from a fraction to a percentage
            int relVol = (int)Math.Round(fRelVol * 100.0f);

            // assume no changes will be made
            bool changed = false;

            // If the device is active, update the existing table entry,
            // or add a new entry if there isn't one already.  If the
            // device is inactive, delete any existing table entry, since
            // we only store active devices.
            if (isActive)
            {
                // The device is active.  If it's already in the list,
                // update the existing entry; otherwise add a new entry.
                if (devices.ContainsKey(id))
                {
                    // It's already in the table.  If the existing entry
                    // has any differences, update it.
                    CfgAudioDevice dev = devices[id];
                    changed = dev.Update(id, name, relVol, useLocal);
                }
                else
                {
                    // It's not in the table - add a new entry
                    CfgAudioDevice dev = new CfgAudioDevice(id, name, relVol, useLocal);
                    devices[id] = dev;
                    changed = true;
                }
            }
            else
            {
                // The device is inactive.  If there's a table entry,
                // delete it.
                if (devices.ContainsKey(id))
                {
                    changed = true;
                    devices.Remove(id);
                }
            }

            // return the change indication
            return changed;
        }

        // Restore configuration settings into the active device list
        public void RestoreAudioDevices(Dictionary<String,UIWin.AudioDevice> sysdevs)
        {
            // Go through the devices found in the configuration.  We only
            // restore settings for devices listed there; devices that are
            // present in the system but not listed in the configuration
            // are left with default settings.  Every device is disabled
            // by default except for the device designated in the Windows
            // audio control panel as the "default" device, which is always
            // enabled.  So the result is that we enable all of the devices
            // explicitly marked as enabled in the configuration, plus the
            // Windows default device.
            foreach (var v in devices)
            {
                // If this device exists in the system devices table, set up
                // the system device with the saved settings.  If it doesn't
                // exist in the system devices table, ignore it.  We leave
                // the entry in the configuration list so that it will still
                // take effect in future sessions if the device is installed
                // again later.
                if (sysdevs.ContainsKey(v.Key))
                {
                    // get the config record and system device entry
                    CfgAudioDevice cfgdev = v.Value;
                    UIWin.AudioDevice sysdev = sysdevs[v.Key];

                    // Copy the config settings to the system device entry.  Note
                    // that isActive is always set to true, since a device only
                    // appears in the config list at all if it's active.
                    sysdev.isActive = true;
                    sysdev.relLevel = cfgdev.relVol / 100.0f;   // adjust % value to normalized scale
                    sysdev.useLocal = cfgdev.useLocal;
                }
            }
        }

        // load the configuration
        public void Load()
        {
            int lineNum = 0;
            try
            {
                String[] lines = File.ReadAllLines(filename);
                foreach (String line in lines)
                {
                    // count it
                    ++lineNum;

                    // skip comments and blank lines
                    if (Regex.IsMatch(line, @"^\s*(#.*)?$"))
                        continue;

                    // check for a key assignment or miscellaneous variable assignment
                    Match m = Regex.Match(line, @"(?i)^\s*(\w+)\s*=\s*(.*)\s*#?");
                    if (m.Success)
                    {
                        // if it's a known key entry, parse the value
                        String varname = m.Groups[1].Value.ToLower();
                        String value = m.Groups[2].Value;

                        try
                        {
                            if (keys.ContainsKey(varname))
                                keys[m.Groups[1].Value].Parse(value, lineNum);
                            else if (varname == "showsettings")
                                ShowSettings = (value.ToLower() == "true" ? true : false);
                            else if (varname == "extvolchanges")
                                ExtVolIsLocal = (value.ToLower() == "local" ? true : false);
                            else if (varname == "osdonhotkeys")
                                OSDOnHotkeys = bool.Parse(value);
                            else if (varname == "osdonappswitch")
                                OSDOnAppSwitch = bool.Parse(value);
                            else if (varname == "osdpos")
                                OSDPos = ParseRectangle(value);
                            else if (varname == "osdrotation")
                                OSDRotation = int.Parse(value);
                            else if (varname == "device")
                                ParseDevice(value);
                            else if (varname == "unmuteonvolchange")
                                UnMuteOnVolChange = bool.Parse(value);
                            else if (varname == "enablejoystick")
                                EnableJoystick = bool.Parse(value);
                            else
                                Log.Error("Invalid key name in config file at line " + lineNum + ": " + m.Groups[1].Value);
                        }
                        catch (Exception exc)
                        {
                            Log.Error("Error parsing config variable " + varname + " at line " + lineNum + ": " + exc.Message);
                        }

                        // we're done with this line
                        continue;
                    }

                    // invalid config line
                    Log.Error("Unrecognized config file entry at line " + lineNum + ": " + line);
                }
            }
            catch (Exception exc)
            {
                Log.Error("Error loading config file" 
                    + (lineNum != 0 ? " at line " + lineNum : "") 
                    + ": " + exc.Message);
            }
        }

        public void Save()
        {
            try
            {
                // set up a list of strings for the data
                List<String> lines = new List<String>();

                // note when we saved the file
                lines.Add("# PinVol config saved " + DateTime.Now.ToString());
                lines.Add("");

                // build the list of key settings
                foreach (KeyValuePair<string, KeyInfo> key in keys)
                    lines.Add(key.Key + " = " + key.Value.Save());

                // add the miscellaneous variables
                lines.Add("ShowSettings = " + ShowSettings);
                lines.Add("ExtVolChanges = " + (ExtVolIsLocal ? "Local" : "Global"));
                lines.Add("OSDOnHotkeys = " + OSDOnHotkeys);
                lines.Add("OSDOnAppSwitch = " + OSDOnAppSwitch);
                lines.Add("OSDPos = " + SaveRectangle(OSDPos));
                lines.Add("OSDRotation = " + OSDRotation);
                lines.Add("UnMuteOnVolChange = " + UnMuteOnVolChange);
                lines.Add("EnableJoystick = " + EnableJoystick);

                // add the active device list
                foreach (var kvp in devices)
                    lines.Add("Device = " + SaveDevice(kvp.Value));

                // write the file
                File.WriteAllLines(filename, lines);
            }
            catch (Exception exc)
            {
                Log.Error("Error saving config file: " + exc.Message);
            }
        }

        String SaveRectangle(Rectangle r)
        {
            return r.X + "," + r.Y + "," + r.Width + "," + r.Height;
        }

        Rectangle ParseRectangle(String s)
        {
            Match m = Regex.Match(s, @"(-?\d+),(-?\d+),(-?\d+),(-?\d+)");
            if (m.Success)
                return new Rectangle(
                    int.Parse(m.Groups[1].Value),
                    int.Parse(m.Groups[2].Value),
                    int.Parse(m.Groups[3].Value),
                    int.Parse(m.Groups[4].Value));
            else
                throw new FormatException("Invalid rectangle format (expected Top,Left,Width,Height, found \"" + s + "\")");
        }

        void ParseDevice(String s)
        {
            Match m = Regex.Match(s, @"(?i)(.*),""((.|"""")*)"",(\d+),(true|false)");
            if (m.Success)
            {
                CfgAudioDevice dev = new CfgAudioDevice(
                    m.Groups[1].Value, 
                    m.Groups[2].Value, 
                    int.Parse(m.Groups[4].Value),
                    bool.Parse(m.Groups[5].Value));
                devices[dev.id] = dev;
            }
            else
                throw new FormatException("Invalid 'device' value format");
        }

        String SaveDevice(CfgAudioDevice dev)
        {
            return dev.id + ","
                + "\"" + dev.name.Replace("\"", "\"\"") + "\","
                + dev.relVol + ","
                + dev.useLocal;
        }

        String filename = "PinVolSettings.ini";
        public Dictionary<String, KeyInfo> keys = new Dictionary<String, KeyInfo>(StringComparer.InvariantCultureIgnoreCase);

        public class KeyInfo
        {
            // initialize with a keyboard key
            public KeyInfo(Keys key, bool shift = false, bool control = false, bool alt = false, bool windows = false)
            {
                this.key = key;
                this.shift = shift;
                this.control = control;
                this.alt = alt;
                this.windows = windows;
                this.jsGuid = Guid.Empty;
                this.jsButton = -1;
            }

            // initialize with a joystick button
            public KeyInfo(Guid guid, int button)
            {
                this.key = Keys.None;
                this.shift = false;
                this.control = false;
                this.alt = false;
                this.windows = false;
                this.jsGuid = guid;
                this.jsButton = button;
            }

            public String ToString(bool includeMods = true)
            {
                // check the type
                if (this.jsGuid != Guid.Empty)
                {
                    // joystick button
                    JoystickDev dev = JoystickDev.FromGuid(jsGuid);
                    String jsName = dev != null ? dev.UnitName : "[Missing Joystick] ";
                    return jsName + " button " + jsButton;
                }
                else
                {
                    // keyboard key

                    // build the modifier list
                    String mod = "";
                    if (includeMods)
                    {
                        if (shift) mod += "Shift+";
                        if (control) mod += "Ctrl+";
                        if (alt) mod += "Alt+";
                        if (windows) mod += "Windows+";
                    }

                    // Get the key name.  Translate "Dn" keys to just "n".  These are
                    // the ordinary digit keys.  The C# enum names have to add the "D" 
                    // prefix to make them valid identifiers for the enum, but we don't
                    // need them or want them in our user-visible representation.
                    String keyname = key.ToString();
                    Match m = Regex.Match(keyname, @"^D(\d)$");
                    if (m.Success)
                        keyname = m.Groups[1].Value;

                    // the full name is the modifier list plus the key name
                    return mod + keyname;
                }
            }

            // string representation for saving in the config file
            public String Save()
            {
                if (jsGuid != Guid.Empty)
                {
                    // joystick button: format is "Joystick{Guid}n", where n is the button number
                    return "Joystick" + jsGuid.ToString("B") + jsButton;
                }
                else
                {
                    // keyboard key: use the printable name with modifiers
                    return ToString(true);
                }
            }

            // parse the config file representation
            public void Parse(String s, int lineNum)
            {
                // clear out any previous key value
                key = Keys.None;
                shift = alt = control = windows = false;
                jsGuid = Guid.Empty;
                jsButton = -1;

                try
                {
                    // try parsing as a key and a joystick button
                    Match mk = Regex.Match(s, @"(?i)(((shift|alt|ctl|ctrl|control|win|windows)\+)*)(.+)");
                    Match mj = Regex.Match(s, @"(?i)Joystick(\{[\da-f\-]+\})(\d+)");

                    // check for a match
                    if (mj.Success)
                    {
                        // joystick button format
                        jsGuid = new Guid(mj.Groups[1].Value);
                        jsButton = int.Parse(mj.Groups[2].Value);
                    }
                    else if (mk.Success)
                    {
                        // keyboard key format

                        // pull out the groups
                        String[] mods = mk.Groups[1].Value.Split('+');
                        String keyname = mk.Groups[4].Value;

                        // parse the modifiers
                        foreach (String mod in mods)
                        {
                            switch (mod.ToLower())
                            {
                                case "shift":
                                    shift = true;
                                    break;
                                case "alt":
                                    alt = true;
                                    break;
                                case "ctl":
                                case "ctrl":
                                case "control":
                                    control = true;
                                    break;
                                case "windows":
                                case "win":
                                    windows = true;
                                    break;
                            }
                        }

                        // If the key name is just a digit, add the "D" prefix for the
                        // C# enumeration.  The enumeration uses D1, D2, etc for the
                        // digit keys because they have to be valid symbol names.
                        if (Regex.IsMatch(keyname, @"^\d+$"))
                            keyname = "D" + keyname;

                        // parse the key name
                        key = (Keys)Enum.Parse(typeof(Keys), keyname, true);
                    }
                    else
                        Log.Error("Error reading config at line " + lineNum + ": invalid key name \"" + s + "\"");
                }
                catch (Exception exc)
                {
                    Log.Error("Error parsing key name \"" + s + "\" in config at line " + lineNum + ": " + exc.Message);
                }
            }

            public void SetKey(Keys key)
            {
                this.key = key;
            }
            public void SetShift(bool f)
            {
                shift = f;
            }
            public void SetControl(bool f)
            {
                control = f;
            }
            public void SetAlt(bool f)
            {
                alt = f;
            }
            public void SetWindows(bool f)
            {
                windows = f;
            }

            // keyboard key data
            public Keys key;
            public bool shift;
            public bool control;
            public bool alt;
            public bool windows;

            // joystick button data
            public Guid jsGuid;
            public int jsButton;
        }
    }
}
