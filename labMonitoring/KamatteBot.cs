using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Twitterizer;

namespace LabMonitoring
{
    /// <summary>
    /// 誰かかまってやれよBOT
    /// </summary>
    class KamatteBot : Logger, ITweetHandler
    {
        /// <summary>
        /// ユーザ別設定など
        /// </summary>
        public readonly KamatteSettings Settings;
        /// <summary>
        /// BOTのTwitterアカウント(@frahabot)のユーザID
        /// </summary>
        public readonly decimal BotUserId = 732414260;
        /// <summary>
        /// 現在監視中のstatus一覧
        /// </summary>
        private DictionaryQueue<decimal, TwitterStatus> watchingList;
        private Timer kamatteCheckTimer;
        private Timer countClearTimer;
        private readonly object lockObj = new object();

        /// <summary>
        /// BOT初期化
        /// ＊ 設定読み込み
        /// ＊ タイマー設定
        /// </summary>
        /// <param name="output">ログ出力先</param>
        public KamatteBot(logOutput output = null)
        {
            LogOutput = output;

            Settings = Properties.Settings.Default.Kamatte;
            if (Settings == null)
            {
                Settings = new KamatteSettings();
                Settings.WaitTime = 5;
                Settings.GlobalFilter = "";
                Settings.Targets = new List<TargetUser>();
                var t = new TargetUser();
                t.Id = 0;
                t.Name = "null";
                t.Filter = "*";
                Settings.Targets.Add(t);
            }
            Log(Settings.ToString());

            watchingList = new DictionaryQueue<decimal, TwitterStatus>();

            /* Set timer */
            countClearTimer = TimerUtil.DailyTimer(CountClearTask);
        }

        public void HandleStatus(TwitterStatus target, logOutput log)
        {
            if (target.User.Id.Equals(BotUserId)) return;

            if (target.InReplyToStatusId != null)
            {
                /* Receive a reply message */
                if (target.User.Id == target.InReplyToUserId)
                {
                    /* Self reply message */
                    if (watchingList.ContainsKey((decimal)target.InReplyToStatusId))
                    {
                        Log("Receive a self reply message: [" + target.Id + "] @" + target.User.ScreenName + " " + target.Text + " to " + target.InReplyToStatusId);
                        Kamatte(target);
                        watchingList.Remove((decimal)target.InReplyToStatusId);
                    }
                }
                else
                {
                    /* Other's tweet */
                    /* A message sent from othe to target */
                    if (watchingList.ContainsKey((decimal)target.InReplyToStatusId))
                    {
                        Log("Receive a message sent from other to target: [" + target.Id + "] @" + target.User.ScreenName + " " + target.Text + " to " + target.InReplyToStatusId);
                        watchingList.Remove((decimal)target.InReplyToStatusId);
                    }
                }
            }
            else
            {
                /* Global target */
                if (Regex.IsMatch(target.Text, Settings.GlobalFilter))
                {
                    if (AddStatusToWatchingList(target))
                    {
                        Log("Receive a global filter tweet: [" + target.Id + "] @" + target.User.ScreenName + " " + target.Text);
                    }
                    return;
                }

                /* Recieve a nomal message */
                if (Settings.GetTargetIdArray().Contains(target.User.Id.ToString()) && target.RetweetedStatus == null)
                {
                    /* Target's tweet */
                    var t = Settings.GetTargetUserFromId(target.User.Id);
                    string f = null;
                    if (t != null)
                    {
                        f = t.Filter;
                    }
                    if (string.IsNullOrEmpty(f))
                    {
                        f = ".*";
                    }

                    if (Regex.IsMatch(target.Text, "(ぼっち|ボッチ)飯"))
                    {
                        Kamatte(target);
                        return;
                    }
                    if (!Regex.IsMatch(target.Text, f, RegexOptions.IgnoreCase)) return;
                    if (Regex.IsMatch(target.Source, ">ikejun<")) return;

                    if (AddStatusToWatchingList(target))
                    {
                        Log("Receive a target tweet: [" + target.Id + "] @" + target.User.ScreenName + " " + target.Text);
                    }
                }
            }
        }

        /// <summary>
        /// watchingListへのスレッドセーフな値の追加
        /// </summary>
        /// <param name="status">追加するTwitterStatus</param>
        /// <returns>追加が成功したかどうか</returns>
        private bool AddStatusToWatchingList(TwitterStatus status)
        {
            try
            {
                watchingList.Add(status.Id, status);
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Trace.WriteLine("WatchingList key is duplicated: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                return false;
            }

            if (kamatteCheckTimer == null)
            {
                kamatteCheckTimer = new Timer(KamatteCheckTask, null, Settings.WaitTime * 60 * 1000, Timeout.Infinite);
                DebugLog("KamatteCheckTimer is set after " + Settings.WaitTime + " minutes.");
            }

            return true;
        }

        /// <summary>
        /// ツッコミ判定用タスク
        /// </summary>
        /// <param name="sender">イベントセンダ</param>
        private void KamatteCheckTask(object sender)
        {
            DebugLog("KamatteCheckTask is started.");
            var now = DateTime.Now;
            TwitterStatus st = null;

            try
            {
                st = watchingList.Peek();
                TimeSpan ts = now - st.CreatedDate;
                while (ts.TotalSeconds > Settings.WaitTime * 60 - 5)
                {
                    Kamatte(watchingList.Dequeue());
                    st = watchingList.Peek();
                    ts = now - st.CreatedDate;
                }
            }
            catch (Exception ex)
            {
                if (!(ex is InvalidOperationException))
                {
                    Log(ex.ToString());
                }
            }

            kamatteCheckTimer.Dispose();
            kamatteCheckTimer = null;

            if (st != null && watchingList.Count > 0)
            {
                DateTime dt = st.CreatedDate.AddMinutes(Settings.WaitTime);
                kamatteCheckTimer = TimerUtil.OnceTimer(KamatteCheckTask, dt);
                DebugLog("KamatteCheckTimer is set at " + dt.ToShortTimeString() + ".");
            }
        }

        /// <summary>
        /// 1日の初めに実行するタスク
        /// </summary>
        /// <param name="sender">イベントセンダ</param>
        private void CountClearTask(object sender)
        {
            lock (lockObj)
            {
                Log("Daily task start");
                Settings.ClearDailyCount();
            }
        }

        /// <summary>
        /// ツッコミ投稿
        /// </summary>
        /// <param name="s">ツッコミ対象status</param>
        private void Kamatte(TwitterStatus s)
        {
            var t = Settings.IncrementKamatteCount(s.User.Id);
            var sb = new StringBuilder("誰かかまってやれよ！ ");
            if (t == null)
            {
                t = new TargetUser();
                t.Id = s.User.Id;
                t.Name = s.User.ScreenName;
                t.DailyKamatteCount = 1;
                t.TotalKamatteCount = 1;
                Settings.Targets.Add(t);
            }
            if (t != null)
            {
                sb.Append("(本日").Append(t.DailyKamatteCount).Append("回目, 累計").Append(t.TotalKamatteCount).Append("回) ");
            }
            sb.Append("RT ").Append(s.User.ScreenName).Append(": ").Append(s.Text.Replace("@", "(at)"));

            var r = Twitter.GetInstance().StatusUpdate(sb.Length > 140 ? sb.ToString(0, 139) + "…" : sb.ToString());
            if (r.Result.Equals(RequestResult.Success))
            {
                Log("Tweet kamatte!: @" + s.User.ScreenName + ", Count: " + t.DailyKamatteCount + "/" + t.TotalKamatteCount);
            }
            else
            {
                Log("Tweet error: " + r.ErrorMessage);
            }
        }

        /// <summary>
        /// デストラクタ
        /// 設定等を保存
        /// </summary>
        ~KamatteBot()
        {
            Properties.Settings.Default.Kamatte = Settings;
            Properties.Settings.Default.Save();
        }
    }
}
