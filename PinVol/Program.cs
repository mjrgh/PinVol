using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PinVol
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
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

    }
}
