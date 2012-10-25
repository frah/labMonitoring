using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Twitterizer;
using Twitterizer.Streaming;

namespace LabMonitoring
{
    /// <summary>
    /// Status処理デリゲート
    /// </summary>
    /// <param name="st">受信したstatus</param>
    /// <param name="log">ログ出力用デリゲート</param>
    delegate void NewStatusHandler(TwitterStatus st, logOutput log);

    /// <summary>
    /// Twitter関連処理クラス
    /// </summary>
    class Twitter
    {
        private OAuthTokens token;
        private TwitterStream ustream;
        private TwitterStream pstream;
        private KamatteSettings k;
        private logOutput l;

        public event NewStatusHandler NewUserStatusEvent;
        public event NewStatusHandler NewPublicStatusEvent;

        /// <summary>
        /// Twitterストリームを初期化
        /// </summary>
        /// <param name="output">ログ出力用Delegate</param>
        public Twitter(logOutput output = null)
        {
            l = output;

            token = new OAuthTokens
            {
                ConsumerKey = Properties.Settings.Default.consumerKey,
                ConsumerSecret = Properties.Settings.Default.consumerSecret,
                AccessToken = Properties.Settings.Default.accessToken,
                AccessTokenSecret = Properties.Settings.Default.accessTokenSecret
            };
            k = Properties.Settings.Default.Kamatte;
            if (k == null)
            {
                k = new KamatteSettings();
                k.WaitTime = 5;
                k.Targets = new List<TargetUser>();
                var t = new TargetUser();
                t.Id = 0;
                t.Name = "null";
                t.Filter = "*";
                k.Targets.Add(t);
            }
            log(k.ToString());

            ustream = new TwitterStream(token, "labMonitor", null);

            var opt = new StreamOptions();
            opt.Follow = k.GetTargetIdArray();
            opt.Track = k.GetTargetNameArray();
            pstream = new TwitterStream(token, "labMonitorPublic", opt);
        }

        /// <summary>
        /// Twitter Streamを開始
        /// </summary>
        public void start()
        {
            try
            {
                ustream.StartUserStream(null, 
                    (x) => { l("UserStream stopped: " + x); },
                    new StatusCreatedCallback(onStatus),
                    null, null, null, null);
                var p = pstream.StartPublicStream(
                    (x) => { l("PublicStream stopped: " + x); },
                    new StatusCreatedCallback(onPStatus),
                    null, null);
                var w = (System.Net.HttpWebRequest)p.AsyncState;
                log(w.Address.ToString());
            }
            catch (TwitterizerException ex)
            {
                throw ex;
            }
            //System.Threading.Thread.Sleep(-1);
        }

        /// <summary>
        /// Twitter Streamを停止
        /// </summary>
        public void end()
        {
            ustream.EndStream();
            pstream.EndStream();
        }

        /// <summary>
        /// UserStream新Status受信時ハンドラ
        /// </summary>
        /// <param name="target">受信したStatus</param>
        private void onStatus(TwitterStatus target)
        {
            if (!target.Text.StartsWith("@frahabot")) return;
            log("@" + target.User.ScreenName + ": " + target.Text);

            NewUserStatusEvent(target, l);
        }

        /// <summary>
        /// PublicStream新Status受信時ハンドラ
        /// </summary>
        /// <param name="target">受信したStatus</param>
        private void onPStatus(TwitterStatus target)
        {
            log("@"+target.User.ScreenName+": "+target.Text);
            NewPublicStatusEvent(target, l);
        }

        /// <summary>
        /// Updateのstaticラッパ
        /// </summary>
        /// <param name="text">本文</param>
        /// <param name="opt">オプション</param>
        /// <returns>投稿結果</returns>
        public static TwitterResponse<TwitterStatus> StatusUpdate(string text, StatusUpdateOptions opt = null)
        {
            var t = new OAuthTokens
            {
                ConsumerKey = Properties.Settings.Default.consumerKey,
                ConsumerSecret = Properties.Settings.Default.consumerSecret,
                AccessToken = Properties.Settings.Default.accessToken,
                AccessTokenSecret = Properties.Settings.Default.accessTokenSecret
            };

            return TwitterStatus.Update(t, text, opt);
        }

        /// <summary>
        /// UpdateWithMediaのstaticラッパ
        /// </summary>
        /// <param name="text">本文</param>
        /// <param name="b">メディア</param>
        /// <param name="opt">オプション</param>
        /// <returns>投稿結果</returns>
        public static TwitterResponse<TwitterStatus> StatusUpdateWithMedia(string text, byte[] b, StatusUpdateOptions opt = null)
        {
            var t = new OAuthTokens
            {
                ConsumerKey = Properties.Settings.Default.consumerKey,
                ConsumerSecret = Properties.Settings.Default.consumerSecret,
                AccessToken = Properties.Settings.Default.accessToken,
                AccessTokenSecret = Properties.Settings.Default.accessTokenSecret
            };

            return TwitterStatus.UpdateWithMedia(t, text, b, opt);
        }

        /// <summary>
        /// デストラクタ
        /// 設定等を保存
        /// </summary>
        ~Twitter()
        {
            Properties.Settings.Default.Kamatte = k;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// ログ出力用関数
        /// </summary>
        /// <param name="str">出力ログ</param>
        private void log(string str)
        {
            if (l != null)
            {
                l(str);
            }
            else
            {
#if DEBUG
                Console.WriteLine(str);
#endif
            }
        }
    }
}

