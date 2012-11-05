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
        private KamatteBot kamatte;
        private logOutput output;
        private List<DailyTask> DailyTasks = new List<DailyTask>();

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
            System.Diagnostics.FileVersionInfo ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Text = ver.ProductName + " " + ver.ProductVersion;

            t = Twitter.GetInstance();
            t.LogOutput = output;
            c = new Camera(output);
            t.NewUserStatusEvent += c.HandleStatus;
            t.NewUserStatusEvent += (a, b) =>
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(a.Text, "ぬ[\n\r]?る[\n\r]?ぽ"))
                {
                    string[] text = { "ｶﾞｯ", "■━⊂(　・∀・) 彡 ｶﾞｯ☆`Д´)ﾉ", "ヽ( ・∀・)ﾉ┌┛ｶﾞｯΣ(ﾉ`Д´)ﾉ" };
                    var opt = new Twitterizer.StatusUpdateOptions();
                    opt.InReplyToStatusId = a.Id;
                    t.StatusUpdate("@" + a.User.ScreenName + " " + text[new Random().Next(text.Length)], opt);
                    b("ｶﾞｯ to @" + a.User.ScreenName);
                }
            };
            t.NewUserStatusEvent += (a, b) =>
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(a.Text, "((眠|ねむ)い|ねむねむ|[寝ね]てない)"))
                {
                    Twitter.GetInstance().StatusUpdate("@" + a.User.ScreenName + " 寝ろ #nero", new Twitterizer.StatusUpdateOptions() { InReplyToStatusId = a.Id });
                }
            };

            kamatte = new KamatteBot(output);
            t.NewPublicStatusEvent += kamatte.HandleStatus;
            t.NewUserStatusEvent += kamatte.HandleStatus;

            try
            {
                t.start();
            }
            catch (Twitterizer.TwitterizerException ex)
            {
                output(ex.Result.ToString());
                output(ex.ErrorDetails.ToString());
                Application.Exit();
            }

            DailyTasks.Add(new CheckCalendar(output));
            DailyTasks.Add(new WeatherPost(output));
            foreach (var d in DailyTasks)
            {
                d.start();
            }
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

            if (logTextBox.Text.Length > logTextBox.MaxLength)
            {
                logTextBox.Text = "";
            }
            logTextBox.HideSelection = false;
            logTextBox.AppendText("[" + DateTime.Now.ToString() + "] " + text + "\r\n");
            logTextBox.SelectionStart = logTextBox.Text.Length;
            logTextBox.Focus();
            logTextBox.ScrollToCaret();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("終了してもよろしいですか？", "終了確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                notifyIcon.Visible = false;
                t.end();
                foreach (var d in DailyTasks)
                {
                    d.stop();
                }
                notifyIcon.Dispose();
                Application.Exit();
            }
        }
    }
}
