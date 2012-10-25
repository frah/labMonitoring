using System;
using Twitterizer;

namespace LabMonitoring
{
    interface ITweetHandler
    {
        /// <summary>
        /// Streamから受け取ったstatusを処理する
        /// </summary>
        /// <param name="target">受け取ったstatus</param>
        /// <param name="log">ログ出力先</param>
        void HandleStatus(TwitterStatus target, logOutput log);
    }
}
