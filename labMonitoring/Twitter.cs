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
    class Twitter : Logger
    {
        private static Twitter instance = new Twitter();
        private OAuthTokens token;
        private TwitterStream ustream;
        private TwitterStream pstream;

        /*
         * nullチェックの手間を省く
         * http://tec.jpn.ph/comp/delegateandevent.html
         */
        public event NewStatusHandler NewUserStatusEvent = delegate(TwitterStatus st, logOutput log) { };
        public event NewStatusHandler NewPublicStatusEvent = delegate(TwitterStatus st, logOutput log) { };

        /// <summary>
        /// Twitterストリームを初期化
        /// </summary>
        /// <param name="output">ログ出力用Delegate</param>
        private Twitter(logOutput output = null)
        {
            LogOutput = output;

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
            opt.Follow = k.GetTargetIdArray();
            opt.Track = k.GetTargetNameArray();
            pstream = new TwitterStream(token, "labMonitorPublic", opt);
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
            try
            {
                ustream.StartUserStream(null, 
                    (x) => { log("UserStream stopped: " + x); },
                    new StatusCreatedCallback(onStatus),
                    null, null, null, null);
                pstream.StartPublicStream(
                    (x) => { log("PublicStream stopped: " + x); },
                    new StatusCreatedCallback(onPStatus),
                    null, null);
            }
            catch (TwitterizerException ex)
            {
                throw ex;
            }
            log("Start Twitter UserStream listening");
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
            NewUserStatusEvent(target, LogOutput);
        }

        /// <summary>
        /// PublicStream新Status受信時ハンドラ
        /// </summary>
        /// <param name="target">受信したStatus</param>
        private void onPStatus(TwitterStatus target)
        {
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
            return null;
#else
            return TwitterStatus.Update(token, text, opt);
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
            return null;
#else
            return TwitterStatus.UpdateWithMedia(token, text, b, opt);
#endif
        }
    }
}

