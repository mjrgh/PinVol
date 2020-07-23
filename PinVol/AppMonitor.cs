using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

// Disable warnings related to unreachable code from const expressions in 
// if statements and ?: expressions, for the sake of DistinguishPBYGames
// conditional code.  We actually *want* the not-selected code to be 
// "unreachable", since that means the compiler won't bother generating 
// code for the unreachable branch or the unchanging condition test.
#pragma warning disable 0162, 0429

namespace PinVol
{
    public class AppMonitor
    {
        // GLOBAL OPTION SETTING:  Do we want to remember separate volume levels
        // for different games in PinballY?  
        //
        // False means that we use the original treatment, where PinVol is treated
        // as a single "game" with a single saved volume level, no matter which
        // game is selected.
        //
        // If you set this to true, PinVol will keep track of the current game
        // selection in PinballY and remember a separate volume level for each
        // game.  (The feature also requires PinballY beta 8 or later, as the 
        // new code to notify PinVol of selection changes was added there.)
        //
        // The default is false, because I think the whole feature turned out to 
        // be a bad idea.  I implemented it to address a PinballY enhancement 
        // request (PinballY issue #9 on github) proposing per-game control over 
        // background media volume levels.  At first glance I thought PinVol would 
        // be an ideal way to address this, since the feature is analogous to 
        // PinVol's per-game volume memory for running games, and because it would
        // nicely conserve keys by using the existing PinVol buttons to adjust the
        // per-game volume.  But on reflection, I think this is the wrong approach. 
        // The problem is that PinballY has both game-specific audio (from the
        // videos and table audio tracks) and "global" audio effects (e.g., from 
        // button presses).  The point of the enhancement request was that some
        // background audio/video tracks in PinballY are too loud and others are
        // too soft, so you want a way to equalize the levels of those tracks.
        // But the global sounds are already inherently equalized since the same
        // sounds are used in all games, so you DON'T want to mess with their
        // levels from game to game.  PinVol can't set those levels separately,
        // though, so if we use PinVol to turn down a table video that's too
        // loud, we get the unwanted side effect of also turning down the
        // button press sounds that were already just right.  So we really need
        // to do any per-game media volume adjustment in PinballY, not in PinVol.
        //
        const bool DistinguishPBYGames = false;

        public AppMonitor()
        {
        }

        // current foreground application name
        public String App {
            get { return app; }
            private set { app = value; }
        }
        String app;

        // Application type.  This gives the class of application for apps that
        // can have multiple distinguished instances with different games, such as 
        // VP or FP.
        String appType = "System";

        public String GetAppType() { return appType; }

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
                else if (appType == "VP")
                {
                    // Visual Pinball - show the game title
                    return Program.mailslotThread.GetGameTitle(app.Substring(3));
                }
                else if (appType == "FP")
                {
                    // Future Pinball - show the game title
                    return Program.mailslotThread.GetGameTitle(app.Substring(3));
                }
                else if (appType == "GameSys")
                {
                    // FP/VP with no game loaded - show the app title
                    return app;
                }
                else if (appType == "PinballY")
                {
                    // If we're distinguishing different game selections in the PinballY
                    // wheel UI, and a game is selected, the app name will be of the form 
                    // PinballY.<game-id>.  If we're not distinguishing games or there's
                    // no game currently selected, app == "PinballY".
                    return app == "PinballY" ? "PinballY" : Program.mailslotThread.GetPBYTitle(app.Substring(9));
                }
                else
                    return app;
            }
        }
        
        // Check to see which application is active.  Returns true if the
        // active app has changed since the last check, false if not.
        public bool CheckActiveApp(Config cfg)
        {
            // assume no change
            String app = App;
            String appType = this.appType;

            // Check for a change in the foreground process.  Also check for a change
            // in window title if the last app was identified as "global".  The global
            // context is a catch-all for windows we can't otherwise identify, so it's
            // possible that it actually is one of our specially identifiable programs,
            // and it just hasn't opened the window by which we can identify it yet,
            // or hasn't set the window title to the recognizable pattern yet.
            //
            // A couple of special cases:
            //
            // - If the app type is "GameSys", meaning a game system window (VP, FP)
            //   with no game loaded, monitor the window for changes in title to tell
            //   us if/when a game is loaded.
            //
            // - If the app type is PinballY, AND we're distinguishing volume levels
            //   for individual game selections in the PinballY wheel UI, check the
            //   mail slot server for an update to the wheel selection.
            //
            IntPtr curwin = GetForegroundWindow();
            int pid, tid = GetWindowThreadProcessId(curwin, out pid);
            if (pid != fgpid 
                || app == GlobalContextName 
                || appType == "GameSys"
                || (DistinguishPBYGames && appType == "PinballY" && Program.mailslotThread.IsPBYSelectionChanged()))
            {
                // switch to the global context, on the assumption that we won't find
                // a window we recognize
                app = GlobalContextName;
                appType = "System";

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
                            appType = "VP";

                            // we've found a suitable window - stop the enumeration
                            return false;
                        }
                        if (title == "Visual Pinball")
                        {
                            // it's a blank Visual Pinball window, with no game loaded
                            app = "Visual Pinball";
                            appType = "GameSys";
                            return false;
                        }
                        if ((m = Regex.Match(title, @"^Future Pinball - \[[^(]*\(\s*(.*)\)\]$")).Success)
                        {
                            // it's a Future Pinball window
                            app = "FP." + Path.GetFileNameWithoutExtension(m.Groups[1].Value);
                            appType = "FP";
                            return false;
                        }
                        if (title == "Future Pinball")
                        {
                            // it's a blank Future Pinball window, with no game loaded
                            app = "Future Pinball";
                            appType = "GameSys";
                            return false;
                        }
                        if (title == "PinballX")
                        {
                            // The PinballX front end is running
                            app = "PinballX";
                            appType = "PinballX";

                            // remember this process ID as PinballX, so that we can recognize
                            // other windows from this process even if they have different names
                            pbxPid = pid;
                            return false;
                        }
                        if (pid == pbxPid)
                        {
                            // the window is part of the PinballX process
                            app = "PinballX";
                            appType = "PinballX";
                            return false;
                        }

                        Func<bool> SetPBY = delegate()
                        {
                            // set the PinballY app type
                            appType = "PinballY";

                            // If we're distinguishing volume levels for different PinballY 
                            // game selections, use the format PinballY.<game-id>.  Otherwise
                            // it's just "PinballY".
                            if (DistinguishPBYGames)
                            {
                                String id = Program.mailslotThread.GetPBYSelection();
                                app = id == "" ? "PinballY" : "PinballY." + id;
                            }
                            else
                                app = "PinballY";

                            return false;
                        };
                        if (title == "PinballY" || title.StartsWith("PinballY -"))
                        {
                            if (GetProcessName() == "pinbally.exe")
                            {
                                // check the class name
                                String wc = GetWindowClassName(hwnd);
                                if (wc != null && wc.StartsWith("PinballY."))
                                {
                                    pbyPid = pid;
                                    return SetPBY();
                                }
                            }
                        }
                        if (pid == pbyPid)
                            return SetPBY();

                        if (title == "PinUP Menu Player")
                        {
                            app = "PinUP Popper";
                            appType = "PinUpPopper";
                            pupPid = pid;
                            return false;
                        }
                        if (pid == pupPid)
                        {
                            app = "PinUP Popper";
                            appType = "PinUpPopper";
                            return false;
                        }

                        if (title == "Pinball FX3" && GetProcessName() == "pinball fx3.exe")
                        {
                            app = "Pinball FX3";
                            appType = "FX3";
                            return false;
                        }

                        if (title == "DEMON'S TILT" && GetProcessName() == "demon's tilt.exe")
                        {
                            app = "Demon's Tilt";
                            appType = "Demon's Tilt";
                            return false;
                        }

                        // check other programs added by the user
                        foreach (var p in cfg.programs)
                        {
                            // check the process name, if specified
                            if (p.exe != null && p.exe != GetProcessName())
                                continue;

                            // check the window title, if specified
                            if (p.windowTitle != null && p.windowTitle != title)
                                continue;

                            // check the window title pattern, if specified
                            Match windowPatternMatch = null;
                            if (p.windowPattern != null && !(windowPatternMatch = p.windowPattern.Match(title)).Success)
                                continue;

                            // It's a match - apply the display name.  Replace $1, $2, etc with
                            // the corresponding match items from the window title pattern, if
                            // supplied.
                            appType = p.appType;
                            app = Regex.Replace(p.displayName, @"\$([1-9&$])", mm => 
                            {
                                // get the $ code
                                String g = mm.Groups[1].Value;

                                // $$ -> literal $
                                if (g == "$")

                                // $& -> full original window title
                                if (g == "&")
                                    return title;

                                // $<digit> -> nth group match
                                int i;
                                if (Int32.TryParse(g, out i) && windowPatternMatch != null && i >= 1 && i < windowPatternMatch.Groups.Count)
                                    return windowPatternMatch.Groups[i].Value;

                                // no substitution - return the original $ sequence literally
                                return mm.Value;
                            });
                        }
                    }

                    // we didn't identify this window - continue the enumeration
                    return true;

                }, IntPtr.Zero);
            }

            // check for a change in the application
            if (app != App)
            {
                // presume this will be an OSD-triggering change
                bool changed = true;

                // remember the new foreground application
                App = app;
                this.appType = appType;
                fgwin = curwin;
                fgpid = pid;

                // return the change indication, to trigger the OSD if needed
                return changed;
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
