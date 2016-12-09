using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PinVol
{
    public class Log
    {
        private static String filename = "PinVol.log";
        public static List<String> messages = new List<String>();

        public static LogViewer viewer;

        // number of errors logged in this session
        public static int nErrors = 0;

        // prune the log: delete the file if it's over the given size
        public static void Prune(long sizeLimit)
        {
            try
            {
                if ((new FileInfo(filename)).Length > sizeLimit)
                    File.Delete(filename);
            }
            catch
            {
            }
        }

        private static void Write(String msg)
        {
            messages.Add(msg);
            File.AppendAllLines(filename, new String[]{ msg });

            if (viewer != null)
                viewer.Add(msg);
        }

        public static void Info(String msg)
        {
            Write(msg); 
        }

        public static void Error(String msg)
        {
            Write(msg);
            ++nErrors;
        }

        public static void ViewLog()
        {
            if (viewer == null)
            {
                viewer = new LogViewer(messages);
                viewer.Show();
            }
            else
                viewer.BringToFront();
        }
    }
}
