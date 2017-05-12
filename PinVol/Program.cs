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
            
            // log startup
            Log.Prune(10 * 1024);
            Log.Info("======================================================================");
            Log.Info("PinVol starting up at " + DateTime.Now.ToString());

            // launch the main window
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UIWin());

            // log shutdown
            Log.Info("Exiting at " + DateTime.Now.ToString());

            // clean up DirectInput resources
            JoystickDev.Cleanup();
        }

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
                // register for the Pinscape config tool broadcast message
                configToolMessage = RegisterWindowMessage("Pincsape.ConfigTool.Running");

                // register for notifications
                Application.AddMessageFilter(this);
            }

            public bool PreFilterMessage(ref Message m)
            {
                // check for our broadcast mesasge
                if (m.Msg == configToolMessage)
                {
                    // note the new status - wparam=1 if running, 0 if terminating
                    configToolRunning = (m.WParam.ToInt32() != 0);

                    // note the message time
                    configToolMessageTime = DateTime.Now;
                }

                // pass it through to the regular message handler
                return false;
            }

            UInt32 configToolMessage;
        }
    }

}
