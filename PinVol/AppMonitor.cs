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
                else if (app.StartsWith("VP."))
                    return Program.mailslotThread.GetGameTitle(app.Substring(3));
                else if (app.StartsWith("FP."))
                    return Program.mailslotThread.GetGameTitle(app.Substring(3));
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
            
            // Check for a change in the foreground process.  Also check for a change
            // in window title if the last app was identified as "global".  The global
            // context is a catch-all for windows we can't otherwise identify, so it's
            // possible that it actually is one of our specially identifiable programs,
            // and it just hasn't opened the window by which we can identify it yet,
            // or hasn't set the window title to the recognizable pattern yet.
            IntPtr curwin = GetForegroundWindow();
            int pid, tid = GetWindowThreadProcessId(curwin, out pid);
            if (pid != fgpid || app == GlobalContextName)
            {
                // switch to the global context, on the assumption that we won't find
                // a window we recognize
                app = GlobalContextName;

                // Check what's running based on the window name.  Scan all of the windows
                // associated with this thread, since the one that we use to identify the
                // application might not be the application's current active window.
                EnumThreadWindows(tid, (IntPtr hwnd, IntPtr lparam) =>
                {
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

                            // we've found a suitable window - stop the enumeration
                            return false;
                        }
                        if ((m = Regex.Match(title, @"^Future Pinball - \[[^(]*\(\s*(.*)\)\]$")).Success)
                        {
                            // it's a Future Pinball window
                            app = "FP." + Path.GetFileNameWithoutExtension(m.Groups[1].Value);
                            return false;
                        }
                        if (title == "PinballX")
                        {
                            // The PinballX front end is running
                            app = "PinballX";

                            // remember this process ID as PinballX, so that we can recognize
                            // other windows from this process even if they have different names
                            pbxPid = pid;
                            return false;
                        }
                        if (pid == pbxPid)
                        {
                            // the window is part of the PinballX process
                            app = "PinballX";
                            return false;
                        }
                        if (title == "PinballY" || title.StartsWith("PinballY -"))
                        {
                            // check the class name
                            String wc = GetWindowClassName(hwnd);
                            if (wc != null && wc.StartsWith("PinballY."))
                            {
                                // the PinballY front end is running
                                app = "PinballY";
                                pbyPid = pid;
                                return false;
                            }
                        }
                        if (pid == pbyPid)
                        {
                            // the window is part of the PinballY process
                            app = "PinballY";
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
                App = app;
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
