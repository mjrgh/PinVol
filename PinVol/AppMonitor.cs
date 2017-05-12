using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Text;
using System.Text.RegularExpressions;

namespace PinVol
{
    public class AppMonitor
    {
        public AppMonitor()
        {
        }

        public String App {
            get { return app; }
            private set { app = value; }
        }
        String app;
        IntPtr fgwin;

        public const String GlobalContextName = "System";

        public String FriendlyName {
            get
            {
                if (app == null)
                    return "None";
                else if (app.StartsWith("VP."))
                    return app.Substring(3);
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
            
            // check for a change in the foreground window
            IntPtr curwin = GetForegroundWindow();
            if (curwin != fgwin)
            {
                // check what's running based on the window name
                int n = 256;
                StringBuilder buf = new StringBuilder(n);
                if (GetWindowText(curwin, buf, n) > 0)
                {
                    // get the process and thread ID for the window
                    int pid, tid = GetWindowThreadProcessId(curwin, out pid);

                    // see what we have for the title
                    String title = buf.ToString();
                    if (title == "Visual Pinball Player")
                    {
                        // The VP player is running.  This window doesn't identify the
                        // running game in its title, but the design window does; it'll
                        // be running in the background in the same thread.  Search for
                        // the designer window and pull the game name out of that.
                        EnumThreadWindows(tid, (IntPtr hwnd, IntPtr lparam) =>
                        {
                            // check the title of this window
                            if (GetWindowText(hwnd, buf, n) > 0)
                            {
                                Match m = Regex.Match(buf.ToString(), @"^Visual Pinball - \[?(.*?)[\]*]*$");
                                if (m.Success)
                                {
                                    // it's the one we're looking for - note it and stop looking
                                    app = "VP." + m.Groups[1].Value;
                                    return false;
                                }
                            }

                            // no match - keep looking
                            return true;

                        }, IntPtr.Zero);
                    }
                    else if (title == "PinballX")
                    {
                        // The PinballX front end is running
                        app = title;

                        // remember this process ID as PinballX, so that we can recognize
                        // other windows from this process even if they have different names
                        pbxPid = pid;
                    }
                    else if (pid == pbxPid)
                    {
                        // the window is part of the PinballX process
                        app = "PinballX";
                    }
                    else
                    {
                        // Nothing we recognize.  Use the global context.
                        app = GlobalContextName;
                    }
                }
            }

            // check for a change in the application
            if (app != App)
            {
                App = app;
                fgwin = curwin;
                return true;
            }

            // no change
            return false;
        }

        // PinballX process ID, if known
        int pbxPid = -1;
        
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
    }
}