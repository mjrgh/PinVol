using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PinVol
{
    public partial class LogViewer : Form
    {
        public LogViewer()
        {
            InitializeComponent();
        }
        public LogViewer(List<String> text)
        {
            InitializeComponent();
            richTextBox1.AppendText(String.Join("\n", text));
        }

        private void LogViewer_Resize(object sender, EventArgs e)
        {
            richTextBox1.Size = ClientSize;
        }

        public void Add(String msg)
        {
            richTextBox1.AppendText(msg + "\n");
        }

        private void LogViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Log.viewer == this)
                Log.viewer = null;
        }
    }
}
