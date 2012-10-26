using System;
using Twitterizer;

namespace LabMonitoring
{
    abstract class ITweetHandler
    {
        public logOutput LogOutput { get; set; }

        /// <summary>
        /// Streamから受け取ったstatusを処理する
        /// </summary>
        /// <param name="target">受け取ったstatus</param>
        /// <param name="log">ログ出力先</param>
        public abstract void HandleStatus(TwitterStatus target, logOutput log);

        /// <summary>
        /// ログ出力用関数
        /// </summary>
        /// <param name="str">出力ログ</param>
        protected void log(string str)
        {
            if (LogOutput != null)
            {
                LogOutput(this.GetType().FullName + "\r\n" + str);
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
