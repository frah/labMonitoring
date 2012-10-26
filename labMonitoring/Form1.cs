using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LabMonitoring;

namespace LabMonitoring
{
    delegate void logOutput(string str);

    public partial class Form1 : Form
    {
        private Twitter t;
        private Camera c;
        private logOutput output;

        private int WM_SYSCOMMAND = 0x112;
        private IntPtr SC_MINIMIZE = (IntPtr)0xF020;

        public Form1()
        {
            InitializeComponent();
            output = new logOutput(_output);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
#if DEBUG
            this.Show();
            this.Activate();
#else
            this.Hide();
#endif

            try
            {
                t = Twitter.GetInstance();
                t.LogOutput = output;
                c = new Camera(output);
                t.NewUserStatusEvent += c.HandleStatus;
                t.start();
            }
            catch (Twitterizer.TwitterizerException ex)
            {
                output(ex.Result.ToString());
                output(ex.ErrorDetails.ToString());
                Application.Exit();
            }
            output("Start Twitter UserStream listening");
        }

        protected override void WndProc(ref Message m)
        {
            /* 
             * 最小化された時にフォームを非表示にする
             * http://pg2se.com/site/2010/03/c---notifyicon.html
             */
            if ((m.Msg == WM_SYSCOMMAND) && (m.WParam == SC_MINIMIZE))
            {
                this.Hide();
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (notifyIcon.Visible)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void Form1_ClientSizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon.Visible = true;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
        }

        public void _output(string text)
        {
            if (this.InvokeRequired)
            {
                Invoke(output, new object[] { text });
                return;
            }

            if (logTextBox.Text.Length > 32767)
            {
                logTextBox.Text = "";
            }
            logTextBox.SelectionStart = logTextBox.Text.Length;
            logTextBox.SelectionLength = 0;
            logTextBox.SelectedText = "[" + DateTime.Now.ToString() + "] " + text + "\r\n";
            logTextBox.SelectionStart = logTextBox.Text.Length;
            logTextBox.SelectionLength = 0;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("終了してもよろしいですか？", "終了確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                notifyIcon.Visible = false;
                t.end();
                notifyIcon.Dispose();
                Application.Exit();
            }
        }
    }
}
