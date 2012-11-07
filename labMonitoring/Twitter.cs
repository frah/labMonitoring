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
    public delegate void NewStatusHandler(TwitterStatus st, logOutput log);

    /// <summary>
    /// Twitter関連処理クラス
    /// </summary>
    public class Twitter : Logger
    {
        const decimal BotUserId = 732414260;
        private static Twitter instance = new Twitter();
        private OAuthTokens token;
        private TwitterStream ustream;
        private TwitterStream pstream;

        /*
         * nullチェックの手間を省く
         * http://tec.jpn.ph/comp/delegateandevent.html
         */
        /// <summary>
        /// User Streamsの新着ツイートイベント
        /// </summary>
        public event NewStatusHandler NewUserStatusEvent = delegate(TwitterStatus st, logOutput log) { };
        /// <summary>
        /// Public Streamsの新着ツイートイベント
        /// </summary>
        public event NewStatusHandler NewPublicStatusEvent = delegate(TwitterStatus st, logOutput log) { };

        /// <summary>
        /// Twitterストリームを初期化
        /// </summary>
        /// <param name="output">ログ出力用Delegate</param>
        private Twitter(logOutput output = null)
        {
            LogOutput = output;

            if (
                string.IsNullOrWhiteSpace(Properties.Settings.Default.consumerKey) ||
                string.IsNullOrWhiteSpace(Properties.Settings.Default.consumerSecret) ||
                string.IsNullOrWhiteSpace(Properties.Settings.Default.accessToken) ||
                string.IsNullOrWhiteSpace(Properties.Settings.Default.accessTokenSecret)
                )
            {
                OAuthForm f = new OAuthForm();
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.Abort)
                {
                    System.Windows.Forms.Application.Exit();
                    Environment.Exit(0);
                }
                Properties.Settings.Default.Reload();
            }

            token = new OAuthTokens
            {
                ConsumerKey = Properties.Settings.Default.consumerKey,
                ConsumerSecret = Properties.Settings.Default.consumerSecret,
                AccessToken = Properties.Settings.Default.accessToken,
                AccessTokenSecret = Properties.Settings.Default.accessTokenSecret
            };

            ustream = new TwitterStream(token, "labMonitor", null);

            var k = Properties.Settings.Default.Kamatte;
            var opt = new StreamOptions();
            if (k != null)
            {
                opt.Follow = k.GetTargetIdArray();
                opt.Track = k.GetTargetNameArray();
                pstream = new TwitterStream(token, "labMonitorPublic", opt);
            }
        }

        /// <summary>
        /// Twitterクラスのインスタンスを返す
        /// </summary>
        /// <returns></returns>
        public static Twitter GetInstance()
        {
            return instance;
        }

        /// <summary>
        /// Twitter Streamを開始
        /// </summary>
        public void start()
        {
            StartUserStream();
            if (pstream != null) StartPublicStream();
            Log("Start Twitter UserStream listening");
        }

        private IAsyncResult StartUserStream()
        {
            try
            {
                return ustream.StartUserStream(null,
                        (x) => { Log("UserStream stopped: " + x); StartUserStream(); },
                        new StatusCreatedCallback(onStatus),
                        null, null, null, null);
            }
            catch (TwitterizerException ex)
            {
                Log(ex.Message);
            }
            return null;
        }
        private IAsyncResult StartPublicStream()
        {
            try
            {
                return pstream.StartPublicStream(
                        (x) => { Log("PublicStream stopped: " + x); StartPublicStream(); },
                        new StatusCreatedCallback(onPStatus),
                        null, null);
            }
            catch (TwitterizerException ex)
            {
                Log(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Twitter Streamを停止
        /// </summary>
        public void end()
        {
            ustream.EndStream();
            if (pstream != null) pstream.EndStream();
        }

        /// <summary>
        /// UserStream新Status受信時ハンドラ
        /// </summary>
        /// <param name="target">受信したStatus</param>
        private void onStatus(TwitterStatus target)
        {
            if (target.User.Id.Equals(BotUserId)) return;
            NewUserStatusEvent(target, LogOutput);
        }

        /// <summary>
        /// PublicStream新Status受信時ハンドラ
        /// </summary>
        /// <param name="target">受信したStatus</param>
        private void onPStatus(TwitterStatus target)
        {
            if (target.User.Id.Equals(BotUserId)) return;
            NewPublicStatusEvent(target, LogOutput);
        }

        /// <summary>
        /// Updateのラッパ
        /// </summary>
        /// <param name="text">本文</param>
        /// <param name="opt">オプション</param>
        /// <returns>投稿結果</returns>
        public TwitterResponse<TwitterStatus> StatusUpdate(string text, StatusUpdateOptions opt = null)
        {
#if DEBUG
            System.Console.WriteLine("[DEBUG] Update status: " + text);
            return new TwitterResponse<TwitterStatus>() { Result = RequestResult.Success };
#else
            var ret = TwitterStatus.Update(token, text, opt);
            if (!ret.Result.Equals(RequestResult.Success))
            {
                DebugLog(string.Format("Twitter update failed: [{0}] {1}", ret.Result, ret.ErrorMessage));
            }
            return ret;
#endif
        }

        /// <summary>
        /// UpdateWithMediaのラッパ
        /// </summary>
        /// <param name="text">本文</param>
        /// <param name="b">メディア</param>
        /// <param name="opt">オプション</param>
        /// <returns>投稿結果</returns>
        public TwitterResponse<TwitterStatus> StatusUpdateWithMedia(string text, byte[] b, StatusUpdateOptions opt = null)
        {
#if DEBUG
            System.Console.WriteLine("[DEBUG] Update status with media: " + text);
            return new TwitterResponse<TwitterStatus>() { Result = RequestResult.Success };
#else
            var ret = TwitterStatus.UpdateWithMedia(token, text, b, opt);
            if (!ret.Result.Equals(RequestResult.Success))
            {
                DebugLog(string.Format("Twitter update failed: [{0}] {1}", ret.Result, ret.ErrorMessage));
            }
            return ret;
#endif
        }
    }
}

