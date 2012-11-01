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
