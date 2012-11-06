using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LabMonitoring
{
    /// <summary>
    /// LabMonitoringのログ出力用抽象クラス
    /// </summary>
    public abstract class Logger
    {
        /// <summary>
        /// ログ出力用デリゲートプロパティ
        /// </summary>
        public logOutput LogOutput { get; set; }

        /// <summary>
        /// ログ出力用関数
        /// </summary>
        /// <param name="str">出力ログ</param>
        protected void Log(string str)
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

        /// <summary>
        /// 条件に一致した時にログを出力
        /// </summary>
        /// <param name="test">条件</param>
        /// <param name="str">出力文字列</param>
        protected void Assert(bool test, string str)
        {
            if (test) Log(str);
        }

        /// <summary>
        /// ファイルのみにログを出力する
        /// </summary>
        /// <param name="str">出力する文字列</param>
        protected void DebugLog(string str)
        {
            Trace.WriteLine("[" + DateTime.Now.ToString() + "] " + this.GetType().FullName + "\r\n" + str);
        }
    }
}
