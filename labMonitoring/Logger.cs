using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LabMonitoring
{
    abstract class Logger
    {
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
    }
}
