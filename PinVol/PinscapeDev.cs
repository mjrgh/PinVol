using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Permissions;

namespace PinVol
{
    class PinscapeDev
    {
        public IntPtr fp;
        public string path;
        public string name;
        public ushort vendorID;
        public ushort productID;
        public ushort version;
        public int LedWizUnitNo;
        public bool JoystickEnabled;
        public bool isValid;
        public ushort inputReportByteLength;

        // Get a list of connected Pinscape Controller devices
        public static List<PinscapeDev> FindDevices()
        {
            // set up an empty return list
            var devices = new List<PinscapeDev>();

            // get the list of devices matching the HID class GUID
            Guid guid = Guid.Empty;
            HIDImports.HidD_GetHidGuid(out guid);
            IntPtr hDevice = HIDImports.SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, HIDImports.DIGCF_DEVICEINTERFACE);

            // set up the attribute structure buffer
            HIDImports.SP_DEVICE_INTERFACE_DATA diData = new HIDImports.SP_DEVICE_INTERFACE_DATA();
            diData.cbSize = Marshal.SizeOf(diData);

            // read the devices in our list
            for (uint i = 0;
                HIDImports.SetupDiEnumDeviceInterfaces(hDevice, IntPtr.Zero, ref guid, i, ref diData);
                ++i)
            {
                // get the size of the detail data structure
                UInt32 size = 0;
                HIDImports.SetupDiGetDeviceInterfaceDetail(hDevice, ref diData, IntPtr.Zero, 0, out size, IntPtr.Zero);

                // now actually read the detail data structure
                HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA diDetail = new HIDImports.SP_DEVICE_INTERFACE_DETAIL_DATA();
                diDetail.cbSize = (IntPtr.Size == 8) ? (uint)8 : (uint)5;
                HIDImports.SP_DEVINFO_DATA devInfoData = new HIDImports.SP_DEVINFO_DATA();
                devInfoData.cbSize = Marshal.SizeOf(devInfoData);
                if (HIDImports.SetupDiGetDeviceInterfaceDetail(hDevice, ref diData, ref diDetail, size, out size, out devInfoData))
                {
                    // create a file handle to access the device
                    IntPtr fp = HIDImports.CreateFile(
                        diDetail.DevicePath, HIDImports.GENERIC_READ_WRITE, HIDImports.SHARE_READ_WRITE,
                        IntPtr.Zero, HIDImports.OPEN_EXISTING, 0, IntPtr.Zero);

                    // read the attributes
                    HIDImports.HIDD_ATTRIBUTES attrs = new HIDImports.HIDD_ATTRIBUTES();
                    attrs.Size = Marshal.SizeOf(attrs);
                    if (HIDImports.HidD_GetAttributes(fp, ref attrs))
                    {
                        // read the product name string
                        String name = "<unknown>";
                        byte[] nameBuf = new byte[128];
                        if (HIDImports.HidD_GetProductString(fp, nameBuf, 128))
                            name = System.Text.Encoding.Unicode.GetString(nameBuf).TrimEnd('\0');

                        // if the vendor and product ID match an LedWiz, and the
                        // product name contains "pinscape", it's one of ours
                        PinscapeDev di;
                        if (CheckIDMatch(attrs)
                            && Regex.IsMatch(name, @"\b(?i)pinscape controller\b")
                            && (di = PinscapeDev.Create(
                                diDetail.DevicePath, name, attrs.VendorID, attrs.ProductID, attrs.VersionNumber)) != null)
                        {
                            // add the device to our list
                            devices.Add(di);
                        }

                        // done with the file handle
                        if (fp.ToInt32() != 0 && fp.ToInt32() != -1)
                            HIDImports.CloseHandle(fp);
                    }
                }
            }

            // done with the device info list
            HIDImports.SetupDiDestroyDeviceInfoList(hDevice);

            // return the device list
            return devices;
        }

        private static PinscapeDev Create(string path, string name, ushort vendorID, ushort productID, ushort version)
        {
            var di = new PinscapeDev(path, name, vendorID, productID, version);
            return di.isValid ? di : null;
        }

        private PinscapeDev(string path, string name, ushort vendorID, ushort productID, ushort version)
        {
            // remember the settings
            this.path = path;
            this.name = name;
            this.vendorID = (ushort)vendorID;
            this.productID = (ushort)productID;        
            this.version = version;
            this.JoystickEnabled = false;
            this.fp = OpenFile();

            // presume invalid
            this.isValid = false;

            // Check the HID interface to see if the HID Usage type is 
            // type 4, for Joystick.  If so, the joystick interface is
            // enabled, which the device uses to send nudge and plunger
            // readings.  If not, the joystick interface is disabled, so
            // the device only sends private status messages and query
            // responses.
            IntPtr preParsedData;
            if (HIDImports.HidD_GetPreparsedData(this.fp, out preParsedData))
            {
                // Check the usage.  If the joystick is enabled, the
                // usage will be 4 = Joystick (on usage page 1, "generic
                // desktop").  If not, the usage is 0 = Undefined, indicating 
                // our private status and query interface.
                HIDImports.HIDP_CAPS caps;
                HIDImports.HidP_GetCaps(preParsedData, out caps);
                if (caps.UsagePage == 1 && caps.Usage == 4)
                    this.JoystickEnabled = true;

                // If the usage is page 1, usage 6, it's a keyboard interface.
                // This type of interface will be exposed alongside the joystick
                // or private interface if any keyboard input is enabled.   Ignore
                // the keyboard interface, since it doesn't accept any control
                // commands - those are strictly for the LedWiz output endpoint
                // that's associated with the joystick or private interface.
                if (caps.UsagePage == 1 && caps.Usage == 6)
                    isValid = false;

                // save the input report size - we have to ask for the correct
                // size when reading input reports
                inputReportByteLength = caps.InputReportByteLength;

                // free the preparsed data
                HIDImports.HidD_FreePreparsedData(preParsedData);
            }

			// read a status report
			byte[] buf = ReadUSB();
			if (buf != null)
			{
				// successfully read a report - mark it as valid
				isValid = true;
			}

			// figure the LedWiz unit number
			LedWizUnitNo = (vendorID == 0xFAFA ? ((productID & 0x0F) + 1) : 0);
        }

        // Check for a USB Product/Vendor ID match to the known values.
        //
        // NB! We're permissive on PID/VID matching.  We'll match ANY IDs, and
        // instead let the caller rely on the product ID string and device query 
        // messages.  We go through the motions of checking for the known ID 
        // codes (the LedWiz codes and our private registered code), but this 
        // is purely for the sake of documentation - we always return true in 
        // the end.
        public static bool CheckIDMatch(HIDImports.HIDD_ATTRIBUTES attrs)
        {
            ushort vid = (ushort)attrs.VendorID;
            ushort pid = (ushort)attrs.ProductID;

            // if it's an LedWiz-compatible code, it's one of ours
            if (vid == 0xFAFA && (pid >= 0x00F0 && pid <= 0x00FF))
                return true;

            // if it's our private Pinscape registration code, it's one of ours
            if (vid == 0x1209 && pid == 0xEAEA)
                return true;

            // It's not one of our known VID/PID combos, but allow it anyway,
            // in case the user is using a custom ID for some reason.  We'll
            // filter out non-Pinscape devices via other, better tests later.
            return true;
        }

        // Query the night mode status
        public bool InNightMode()
        {
            // read a joystick report
            byte[] buf = ReadUSB();

            // if that failed, return the previous status
            if (buf == null)
                return nightModeStatus;

            // only pay attention to regular status reports - the high
            // bit of the second byte must be zero
            if ((buf[2] & 0x80) != 0)
                return nightModeStatus;

            // Parse the report and remember the new status.  The parts 
            // we're interested in are:
            //
            //  [0] = USB report ID.  Always 0.
            //  [1] = Status byte 0.  Bit fields:
            //          0x02 -> night mode engaged
            const byte NIGHT_MODE_BIT = 0x02;
            nightModeStatus = (buf[1] & NIGHT_MODE_BIT) != 0;

            // return the new status
            return nightModeStatus;
        }

        // last night mode status
        bool nightModeStatus = false;

        private IntPtr OpenFile()
        {
            return HIDImports.CreateFile(
                path, HIDImports.GENERIC_READ_WRITE, HIDImports.SHARE_READ_WRITE,
                IntPtr.Zero, HIDImports.OPEN_EXISTING, HIDImports.EFileAttributes.Overlapped, IntPtr.Zero);
        }

        private bool TryReopenHandle()
        {
            // if the last error is 6 ("invalid handle") or 1167 ("Device not connected"), 
            // try re-opening the handle
            int err = Marshal.GetLastWin32Error();
            if (err == 6 || err == 1167)
            {
                // try opening a new handle on the device path
                Console.WriteLine("invalid handle on read/write - trying to reopen");
                IntPtr fp2 = OpenFile();

                // if that succeeded, replace the old handle with the new one and retry the read
                if (fp2 != null)
                {
                    // replace the handle
                    fp = fp2;

                    // tell the caller to try again
                    return true;
                }
            }

            // we didn't successfully reopen the handle
            return false;
        }

        public String GetLastWin32ErrMsg()
        {
            int errNo = Marshal.GetLastWin32Error();
            return String.Format("{0} (Win32 error {1})",
                new System.ComponentModel.Win32Exception(errNo).Message, errNo);
        }

        private NativeOverlapped ov;
        public byte[] ReadUSB()
        {
            // try reading a few times, in case the connection drops momentarily
            for (int tries = 0; tries < 3; ++tries)
            {
                // set up a non-blocking ("overlapped") read
                byte[] buf = new byte[inputReportByteLength];
                buf[0] = 0x00;
                EventWaitHandle ev = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
                ov.OffsetLow = ov.OffsetHigh = 0;
                ov.EventHandle = ev.SafeWaitHandle.DangerousGetHandle();
                HIDImports.ReadFile(fp, buf, inputReportByteLength, IntPtr.Zero, ref ov);

                // Wait briefly for the read to complete.  But don't wait forever - we might
                // be talking to a device interface that doesn't provide the type of status
                // report we're looking for, in which case we don't want to get stuck waiting
                // for something that will never happen.  If this is indeed the controller
                // interface we're interested in, it will respond within a few milliseconds
                // with our status report.
                if (ev.WaitOne(100))
                {
                    // The read completed successfully!  Get the result.
                    UInt32 readLen;
                    if (HIDImports.GetOverlappedResult(fp, ref ov, out readLen, 0) == 0)
                    {
                        // The read failed.  Try re-opening the file handle in case we
                        // dropped the connection, then re-try the whole read.
                        TryReopenHandle();
                        continue;
                    }
                    else if (readLen != inputReportByteLength)
                    {
                        // The read length didn't match what we expected.  This might be
                        // a different device (not a Pinscape controller) or a different
                        // version that we don't know how to talk to.  In either case,
                        // return failure.
                        return null;
                    }
                    else
                    {
                        // The read succeed and was the correct size.  Return the data.
                        return buf;
                    }
                }
                else
                {
                    // The read timed out.  This must not be our control interface after
                    // all.  Cancel the read and try reopening the handle.
                    HIDImports.CancelIo(fp);
                    if (TryReopenHandle())
                        continue;
                    return null;
                }
            }

            // don't retry more than a few times
            return null;
        }
    }
}

