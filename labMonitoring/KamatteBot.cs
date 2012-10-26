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
    class KamatteBot : ITweetHandler
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
        private Dictionary<decimal, TwitterStatus> watchingList;
        private Timer kamatteCheckTimer;
        private Timer countClearTimer;

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
                Settings.Targets = new List<TargetUser>();
                var t = new TargetUser();
                t.Id = 0;
                t.Name = "null";
                t.Filter = "*";
                Settings.Targets.Add(t);
            }
            log(Settings.ToString());

            watchingList = new Dictionary<decimal, TwitterStatus>();

            /* Set timer */
            kamatteCheckTimer = new Timer(KamatteCheckTask, null, 60000, 60000);
            TimeSpan ts = DateTime.Parse(DateTime.Now.AddDays(1).ToShortDateString()) - DateTime.Now;
            countClearTimer = new Timer(CountClearTask, null, ts, new TimeSpan(24, 0, 0));
        }

        public override void HandleStatus(TwitterStatus target, logOutput log)
        {
            if (target.InReplyToStatusId != null)
            {
                /* Receive a reply message */
                if (Settings.GetTargetIdArray().Contains(target.InReplyToUserId.ToString()))
                {
                    if (target.User.Id == target.InReplyToUserId)
                    {
                        /* Self reply message */
                        log("Receive a self reply message: [" + target.Id + "] @" + target.User.ScreenName + " " + target.Text + " to " + target.InReplyToStatusId);
                        Kamatte(target);
                        if (watchingList.ContainsKey((decimal)target.InReplyToStatusId))
                        {
                            watchingList.Remove((decimal)target.InReplyToStatusId);
                        }
                    }
                    else
                    {
                        /* Other's tweet */
                        /* A message sent from othe to target */
                        log("Receive a message sent from other to target: [" + target.Id + "] @" + target.User.ScreenName + " " + target.Text + " to " + target.InReplyToStatusId);
                        if (watchingList.ContainsKey((decimal)target.InReplyToStatusId))
                        {
                            watchingList.Remove((decimal)target.InReplyToStatusId);
                        }
                    }
                }
            }
            else
            {
                /* Recieve a nomal message */
                if (Settings.GetTargetIdArray().Contains(target.User.Id.ToString()) && !target.Retweeted)
                {
                    /* Target's tweet */
                    var t = Settings.GetTargetUserFromId(target.User.Id);
                    string f = null;
                    if (t != null)
                    {
                        f = t.Filter;
                    }
                    if (f == null || f.Equals(""))
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

                    log("Receive a target tweet: [" + target.Id + "] @" + target.User.ScreenName + " " + target.Text);
                    watchingList.Add(target.Id, target);
                }
            }
        }

        /// <summary>
        /// ツッコミ判定用タスク
        /// </summary>
        /// <param name="sender">イベントセンダ</param>
        private void KamatteCheckTask(object sender)
        {
            log("ツッコミ判定スタート");
            var now = DateTime.Now;
            for (int i = watchingList.Count - 1; i >= 0; i--)
            {
                TimeSpan ts = now - watchingList[i].CreatedDate;
                if (ts.TotalMinutes > Settings.WaitTime)
                {
                    Kamatte(watchingList[i]);
                    watchingList.Remove(watchingList[i].Id);
                }
            }
        }

        /// <summary>
        /// 1日の初めに実行するタスク
        /// </summary>
        /// <param name="sender">イベントセンダ</param>
        private void CountClearTask(object sender)
        {
            Settings.ClearDailyCount();
        }

        /// <summary>
        /// ツッコミ投稿
        /// </summary>
        /// <param name="s">ツッコミ対象status</param>
        private void Kamatte(TwitterStatus s)
        {
            var t = Settings.IncrementKamatteCount(s.User.Id);
            if (t != null)
            {
                var sb = new StringBuilder("誰かかまってやれよ！ ");
                sb.Append("(本日").Append(t.DailyKamatteCount).Append("回目, 累計").Append(t.TotalKamatteCount).Append("回) ");
                sb.Append("RT ").Append(s.User.ScreenName).Append(": ").Append(s.Text);

                var r = Twitter.GetInstance().StatusUpdate(sb.Length > 140 ? sb.ToString(0, 139) + "…" : sb.ToString());
                if (r.Result.Equals(RequestResult.Success))
                {
                    log("Tweet kamatte!: @" + s.User.ScreenName + ", Count: " + t.DailyKamatteCount + "/" + t.TotalKamatteCount);
                }
                else
                {
                    log("Tweet error: " + r.ErrorMessage);
                }
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
