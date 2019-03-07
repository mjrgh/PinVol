using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace PinVol
{
    public static class Program
    {
        [DllImport("User32.dll",
            EntryPoint = "RegisterWindowMessage",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern UInt32 RegisterWindowMessage(String str);

        [STAThread]
        static void Main()
        {
            // set up our Config Tool broadcast message receiver
            ConfigToolMessageFilter cfgToolFilter = new ConfigToolMessageFilter();

            // set up our mailbox monitor thread
            mailslotThread = new MailslotThread();
            mailslotThread.Start();
            
            // log startup
            Log.Prune(10 * 1024);
            Log.Info("======================================================================");
            Log.Info("PinVol starting up at " + DateTime.Now.ToString());

            // launch the main window
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UIWin());

            // shut down the mailslot server thread
            mailslotThread.Shutdown();

            // log shutdown
            Log.Info("Exiting at " + DateTime.Now.ToString());

            // clean up DirectInput resources
            JoystickDev.Cleanup();
        }

        // Mailslot monitor thread
        public class MailslotThread
        {
            public MailslotThread() { }

            public void Start()
            {
                thread = new Thread(new ThreadStart(ThreadMain));
                thread.Start();
            }

            // Look up the game title for a filename.  This looks for a mapping
            // provided by the front end system (e.g., PinballY).
            public String GetGameTitle(String filename)
            {
                // use the lowercase version of the name as the key
                String key = filename.ToLower();

                // lock the map while doing the lookup
                lock (locker)
                {
                    // if the filename is in the map, return the associated title
                    if (gameNameMap.ContainsKey(key))
                        return gameNameMap[key];

                    // no title - return the original filename
                    return filename;
                }
            }

            // Has the PinballY selection changed since the last time this was
            // called?  If so, this returns true and clears the flag.
            public bool IsPBYSelectionChanged()
            {
                lock (locker)
                {
                    bool changed = pbySelectionChanged;
                    pbySelectionChanged = false;
                    return changed;
                }
            }

            // Get the current PinballY game name.  PinballY informs us via mailslot
            // messages which game is selected in its UI, so that we can set different
            // separate volume levels per game.
            public String GetPBYSelection()
            {
                lock (locker)
                    return pbySelectionId;
            }

            // Given a PinballY selection, get the associated game title
            public String GetPBYTitle(String id)
            {
                lock (locker)
                {
                    // try looking up the ID in the table
                    if (pbyNameMap.ContainsKey(id))
                        return pbyNameMap[id];

                    return id;
                }
            }

            // Shut down the server thread
            public void Shutdown()
            {
                // set the shutdown flag - the thread will read this and exit the
                // next time it wakes up to read a message
                shuttingDown = true;

                // Create a client for our own slot, so that we can send ourselves
                // a message to make sure we break out of the loop.  Note that the
                // message itself is meaningless, so we send a simple "nop" (no 
                // operation) message.  The message's purpose is just to wake up
                // the server thread by putting something in the slot that it can
                // read.  The server thread just sits there blocking on the a read
                // call most of the time, so the way to unblock it is to allow the
                // read to complete.
                var client = new MailslotClient(slotName);
                client.SendMessage("nop");

                // Wait for the thread to exit, with a timeout to make sure we don't
                // get stuck if the thread is stuck.  The timeout doesn't need to be
                // long; the thread does very little work on each iteration, so if
                // it doesn't exit almost immediately, it probably never will.
                thread.Join(500);
            }

            // thread main
            void ThreadMain()
            {
                // create our mailslot
                var server = new MailslotServer(slotName);

                // Read input in a loop until the shutdown flag is set
                while (!shuttingDown)
                {
                    // read the next message; ignore null messages
                    string msg = server.GetNextMessage();
                    if (msg == null)
                        continue;

                    // check what we have
                    Match m;
                    if ((m = Regex.Match(msg, @"^game ([^|]+)\|(.*)$")).Success)
                    {
                        // Game file to title mapping: "game filename|title".  The filename
                        // can't contain a '|' character, so this is an unambiguous separator.
                        String filename = m.Groups[1].Value.ToLower();
                        String title = m.Groups[2].Value;

                        // add it to the game name table
                        lock (locker) 
                            gameNameMap[filename] = title;

                        // If the filename has an extension, add an entry sans extension as well.
                        // We have to infer the current game file from the foreground app's window
                        // title, and some apps (such as VP) only include the root filename without
                        // an extension.
                        if ((m = Regex.Match(filename, @"(.*)\.[^.]+$")).Success)
                        {
                            lock (locker)
                                gameNameMap[m.Groups[1].Value] = title;
                        }
                    }
                    else if ((m = Regex.Match(msg, @"^PinballY Select (.*)\n(.*)$")).Success)
                    {
                        // PinballY wheel selection - store the new game selection
                        lock (locker)
                        {
                            pbySelectionId = m.Groups[1].Value;
                            pbyNameMap[pbySelectionId] = m.Groups[2].Value;
                            pbySelectionChanged = true;
                        }
                    }
                    else if (msg == "PinballY SelectNone")
                    {
                        lock (locker)
                        {
                            pbySelectionId = "";
                            pbySelectionChanged = true;
                        }
                    }
                }
            }

            // mailslot name
            static String slotName = "Pinscape.PinVol";

            // thread object representing the mailslot read thread
            Thread thread;

            // flag: the main program is shutting down; the reader thread will exit the
            // next time it wakes up from a read if this is true
            volatile bool shuttingDown = false;

            // locker for the game name table
            Object locker = new Object();

            // Game name table.  This is a map of filename -> game title.  Front ends (such
            // as PinballY) that keep track of game metadata can send us these name mappings 
            // so that we can show the game's friendly title in the OSD, instead of the raw
            // filename.  The filename keys are converted to lowercase for more liberal
            // matching.
            Dictionary<String, String> gameNameMap = new Dictionary<String, String>();

            // Current PinballY game ID 
            String pbySelectionId = "";

            // PinballY game name table
            Dictionary<String, String> pbyNameMap = new Dictionary<String, String>();

            // Has the PBY selection changed since the last time it was checked?
            bool pbySelectionChanged = false;
        }

        // mailbox thread singleton
        public static MailslotThread mailslotThread;

        // is the config tool running?
        public static bool configToolRunning = false;

        // time of last config tool notification
        static DateTime configToolMessageTime = DateTime.Now;

        // check for config tool notification timeout
        public static void configToolTimeout()
        {
            if (configToolRunning && (DateTime.Now - configToolMessageTime).TotalSeconds > 5)
                configToolRunning = false;
        }

        // Config tool broadcast message filter
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public class ConfigToolMessageFilter : IMessageFilter
        {
            public ConfigToolMessageFilter()
            {
                // Register for the Pinscape config tool broadcast message.  An earlier
                // version had an embarrassing typo here, which was replicated in the config
                // tool through the diabolical miracle of copy-and-paste, so alas, we need
                // to recognize both messages for compatibility with older config tool
                // releases.
                configToolMessage1 = RegisterWindowMessage("Pinscape.ConfigTool.Running");
                configToolMessage2 = RegisterWindowMessage("Pincsape.ConfigTool.Running");  // [sic - Pin*cs*ape - the grandfathered typo]

                // register for notifications
                Application.AddMessageFilter(this);
            }

            public bool PreFilterMessage(ref Message m)
            {
                // check for our broadcast mesasge
                if (m.Msg == configToolMessage1 || m.Msg == configToolMessage2)
                {
                    // note the new status - wparam=1 if running, 0 if terminating
                    configToolRunning = (m.WParam.ToInt32() != 0);

                    // note the message time
                    configToolMessageTime = DateTime.Now;
                }

                // pass it through to the regular message handler
                return false;
            }

            UInt32 configToolMessage1;
            UInt32 configToolMessage2;
        }
    }

}
