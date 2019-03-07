using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace PinVol
{
    public class AppMonitor
    {
        public AppMonitor()
        {
        }

        // current foreground application name
        public String App {
            get { return app; }
            private set { app = value; }
        }
        String app;

        // current foreground application type
        public enum AppType
        {
            System,  // any unknown app type
            VP,
            FP,
            PinballX,
            PinballY,
            Other    // other known app types not included above
        };
        AppType appType = AppType.System;

        public AppType GetAppType() { return appType; }

        // current foreground HWND
        IntPtr fgwin = IntPtr.Zero;
        
        // current foreground process ID
        int fgpid = 0;

        public const String GlobalContextName = "System";

        public String FriendlyName {
            get
            {
                if (app == null)
                    return "None";
                else if (appType == AppType.VP)
                    return app == "VP" ? "Visual Pinball" : Program.mailslotThread.GetGameTitle(app.Substring(3));
                else if (appType == AppType.FP)
                    return Program.mailslotThread.GetGameTitle(app.Substring(3));
                else if (appType == AppType.PinballY)
                    return app == "PinballY" ? "PinballY" : Program.mailslotThread.GetPBYTitle(app.Substring(9));
                else
                    return app;
            }
        }
        
        // Check to see which application is active.  Returns true if the
        // active app has changed since the last check, false if not.
        public bool CheckActiveApp()
        {
            // assume no change
            String app = App;
            AppType appType = this.appType;

            // Check for a change in the foreground process.  Also check for a change
            // in window title if the last app was identified as "global".  The global
            // context is a catch-all for windows we can't otherwise identify, so it's
            // possible that it actually is one of our specially identifiable programs,
            // and it just hasn't opened the window by which we can identify it yet,
            // or hasn't set the window title to the recognizable pattern yet.
            IntPtr curwin = GetForegroundWindow();
            int pid, tid = GetWindowThreadProcessId(curwin, out pid);
            if (pid != fgpid 
                || app == GlobalContextName 
                || app == "VP"
                || (appType == AppType.PinballY && Program.mailslotThread.IsPBYSelectionChanged()))
            {
                // switch to the global context, on the assumption that we won't find
                // a window we recognize
                app = GlobalContextName;
                appType = AppType.System;

                // Check what's running based on the window name.  Scan all of the windows
                // associated with this thread, since the one that we use to identify the
                // application might not be the application's current active window.
                EnumThreadWindows(tid, (IntPtr hwnd, IntPtr lparam) =>
                {
                    // we'll inspect the process name in some cases; cache it if we get if
                    // for this window so that we don't have to get it more than once
                    String processName = null;
                    Func<String> GetProcessName = delegate()
                    {
                        // if we haven't already retrieved the process name, do so now
                        if (processName == null)
                        {
                            // open the process for limited information query
                            IntPtr hProc = OpenProcess(ProcessQueryLimitedInformation, false, pid);
                            if (hProc != IntPtr.Zero)
                            {
                                // get the image name
                                int procNameBufMax = 256;
                                StringBuilder procNameBuf = new StringBuilder(procNameBufMax);
                                if (GetProcessImageFileNameW(hProc, procNameBuf, procNameBufMax) != 0)
                                {
                                    // got it - save it for next time
                                    processName = Path.GetFileName(procNameBuf.ToString()).ToLower();
                                }
                            }
                        }

                        // return the cacched information
                        return processName;
                    };

                    // get the window caption
                    int n = 256;
                    StringBuilder buf = new StringBuilder(n);
                    if (GetWindowText(hwnd, buf, n) > 0)
                    {
                        // look for captions matching the pinball player and front end apps
                        String title = buf.ToString();
                        Match m;
                        if ((m = Regex.Match(title, @"^Visual Pinball - \[?(.*?)[\]*]*$")).Success)
                        {
                            // it's a Visual Pinball window
                            app = "VP." + m.Groups[1].Value;
                            appType = AppType.VP;

                            // we've found a suitable window - stop the enumeration
                            return false;
                        }
                        if ((m = Regex.Match(title, @"^Future Pinball - \[[^(]*\(\s*(.*)\)\]$")).Success)
                        {
                            // it's a Future Pinball window
                            app = "FP." + Path.GetFileNameWithoutExtension(m.Groups[1].Value);
                            appType = AppType.FP;
                            return false;
                        }

                        if (title == "PinballX")
                        {
                            // The PinballX front end is running
                            app = "PinballX";
                            appType = AppType.PinballX;

                            // remember this process ID as PinballX, so that we can recognize
                            // other windows from this process even if they have different names
                            pbxPid = pid;
                            return false;
                        }
                        if (pid == pbxPid)
                        {
                            // the window is part of the PinballX process
                            app = "PinballX";
                            appType = AppType.PinballX;
                            return false;
                        }

                        if (title == "PinballY" || title.StartsWith("PinballY -"))
                        {
                            if (GetProcessName() == "pinbally.exe")
                            {
                                // check the class name
                                String wc = GetWindowClassName(hwnd);
                                if (wc != null && wc.StartsWith("PinballY."))
                                {
                                    // the PinballY front end is running
                                    String id = Program.mailslotThread.GetPBYSelection();
                                    app = id == "" ? "PinballY" : "PinballY." + id;
                                    appType = AppType.PinballY;
                                    pbyPid = pid;
                                    return false;
                                }
                            }
                        }
                        if (pid == pbyPid)
                        {
                            // the window is part of the PinballY process
                            String id = Program.mailslotThread.GetPBYSelection();
                            app = id == "" ? "PinballY" : "PinballY." + id;
                            appType = AppType.PinballY;
                            return false;
                        }

                        if (title == "PinUP Menu Player")
                        {
                            app = "PinUP Popper";
                            appType = AppType.Other;
                            pupPid = pid;
                            return false;
                        }
                        if (pid == pupPid)
                        {
                            app = "PinUP Popper";
                            appType = AppType.Other;
                            return false;
                        }

                        if (title == "Pinball FX3" && GetProcessName() == "pinball fx3.exe")
                        {
                            app = "Pinball FX3";
                            appType = AppType.Other;
                            return false;
                        }

                        if (title == "DEMON'S TILT" && GetProcessName() == "demon's tilt.exe")
                        {
                            app = "Demon's Tilt";
                            appType = AppType.Other;
                            return false;
                        }
                    }

                    // we didn't identify this window - continue the enumeration
                    return true;

                }, IntPtr.Zero);
            }

            // check for a change in the application
            if (app != App)
            {
                // If we didn't identify the app more specifically than "System", check
                // a special case.  When Visual Pinball first starts in /play mode, it
                // opens a window titled just "Visual Pinball", with no document loaded.
                if (appType == AppType.System && curwin != IntPtr.Zero)
                {
                    // get the window title
                    int n = 256;
                    StringBuilder buf = new StringBuilder(n);
                    if (GetWindowText(curwin, buf, n) > 0 && buf.ToString() == "Visual Pinball")
                    {
                        appType = AppType.VP;
                        app = "VP";
                    }
                }

                // remember the new foreground application
                App = app;
                this.appType = appType;
                fgwin = curwin;
                fgpid = pid;
                return true;
            }

            // no change
            return false;
        }

        // PinballX process ID, if known
        int pbxPid = -1;

        // PinballY process ID, if known
        int pbyPid = -1;

        // PinUP Popper process ID, if known
        int pupPid = -1;
        
        // Win32 imports

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        delegate bool EnumThreadWindowsDelegate(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadWindowsDelegate callback, IntPtr lparam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        // process access rights
        const int AccessDelete = 0x00010000;
        const int AccessReadControl = 0x00020000;
        const int AccessSynchronize = 0x00100000;
        const int AccessWriteDac = 0x00040000;
        const int AccessWriteOnwer = 0x00080000;
        const int ProcessAllAccess = -1;
        const int ProcessCreateProcess = 0x0080;
        const int ProcessCreateThread = 0x0002;
        const int ProcessDupHandle = 0x0040;
        const int ProcessQueryInformation = 0x0400;
        const int ProcessQueryLimitedInformation = 0x1000;
        const int ProcessSetInformation = 0x0200;
        const int ProcessSetQuota = 0x0100;
        const int ProcessSuspendResume = 0x0800;
        const int ProcessTerminate = 0x0001;
        const int ProcessVMOperation = 0x0008;
        const int ProcessVMRead = 0x0010;
        const int ProcessVMWrite = 0x0020;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Psapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetProcessImageFileNameW(IntPtr hProc, StringBuilder lpImageFileName, int nSize);

        static String GetWindowClassName(IntPtr hwnd)
        {
            try
            {
                int maxLen = 1000;
                StringBuilder s = new StringBuilder(null, maxLen + 3);
                GetClassName(hwnd, s, maxLen);
                return s.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
