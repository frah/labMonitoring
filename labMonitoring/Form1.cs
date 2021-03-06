﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LabMonitoring;

namespace LabMonitoring
{
    /// <summary>
    /// ログ出力用デリゲート
    /// </summary>
    /// <param name="str">出力ログ</param>
    public delegate void logOutput(string str);
    /// <summary>
    /// 現在のログ出力を返す
    /// </summary>
    /// <returns>現在のログ</returns>
    public delegate string currentLog();

    /// <summary>
    /// メインウィンドウ
    /// </summary>
    public partial class Form1 : Form
    {
        private Twitter t;
        private Camera c;
        private KamatteBot kamatte;
        private logOutput output;
        private currentLog curLog;
        private List<DailyTask> DailyTasks = new List<DailyTask>();
        private HttpServer http;
        private IrcController irc;

        private int WM_SYSCOMMAND = 0x112;
        private IntPtr SC_MINIMIZE = (IntPtr)0xF020;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            output = new logOutput(_output);
            curLog = new currentLog(_curLog);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
#if DEBUG
            this.Show();
            this.Activate();
#else
            this.Hide();
#endif
            FileVersionInfo ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Text = ver.ProductName + " " + ver.ProductVersion;
#if DEBUG
            this.Text = this.Text + " - DEBUG";
#endif

            new Thread(InitThread).Start();
        }

        /// <summary>
        /// 初期化処理用スレッド
        /// </summary>
        private void InitThread()
        {
            t = Twitter.GetInstance();
            t.LogOutput = output;
            c = new Camera(output);
            t.NewUserStatusEvent += c.HandleStatus;
            t.AddSimpleReplyUserBot("ぬ[\n\r]?る[\n\r]?ぽ", "ｶﾞｯ", "■━⊂(　・∀・) 彡 ｶﾞｯ☆`Д´)ﾉ", "ヽ( ・∀・)ﾉ┌┛ｶﾞｯΣ(ﾉ`Д´)ﾉ");
            t.AddSimpleReplyUserBot("((眠|ねむ)い|ねむねむ|[寝ね]てない|寝不足)",
                (x) => {
                    string[] kao = { "", "(☝ ՞ਊ ՞)☝", "(´◉◞౪◟◉)", "(´☣益☣)" };
                    return kao[new Random().Next(kao.Length)] + " 寝ろ #nero";
                }
            );
            t.AddSimpleUserBot("(暴発)",
                (x) => {
                    string[] kao = { "(╯⊙ ⊱ ⊙╰ )", "(◉◞౪◟◉｀)", "ʅ(◔౪◔ ) ʃ", "✌(՞ਊ ՞)✌" };
                    return string.Format("{0}> {1} (by {2})", x.Text, kao[new Random().Next(kao.Length)], x.User.ScreenName);
                }
            );

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

            http = new HttpServer() { LogOutput = output, CurLog = curLog };
            http.Start();
            try
            {
                irc = new IrcController() { LogOutput = output, CurLog = curLog };
                irc.Start();
            }
            catch (ArgumentNullException)
            {
                output("IRC controller init failed.");
            }
        }

        /// <summary>
        /// Windowsメッセージを処理する
        /// </summary>
        /// <param name="m">Windowsメッセージ</param>
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

        /// <summary>
        /// テキストボックスとTraceログに与えられた文字列を出力する
        /// </summary>
        /// <param name="text">出力する文字列</param>
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
            string logText = "[" + DateTime.Now.ToString() + "] " + text + "\r\n";
            logTextBox.HideSelection = false;
            logTextBox.AppendText(logText);
            logTextBox.SelectionStart = logTextBox.Text.Length;
            logTextBox.Focus();
            logTextBox.ScrollToCaret();
            Trace.Write(logText);
        }

        /// <summary>
        /// 現在のテキストボックスの内容を返す
        /// </summary>
        /// <returns>テキストボックスの内容</returns>
        public string _curLog()
        {
            if (this.InvokeRequired)
            {
                return Invoke(curLog) as string;
            }

            return logTextBox.Text;
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
                http.Stop();
                if (irc != null) irc.Stop();
                notifyIcon.Dispose();
                Application.Exit();
                Environment.Exit(0);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Kamatte = kamatte.Settings;
            Properties.Settings.Default.Save();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            kamatte.ReloadSetting();
        }
    }
}
