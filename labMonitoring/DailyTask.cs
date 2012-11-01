using System;
using System.Threading;

namespace LabMonitoring
{
    /// <summary>
    /// 1日1回のタスククラス
    /// runメソッドの内容がプロパティで指定された時間に毎日実行される
    /// </summary>
    abstract class DailyTask : Logger
    {
        public Int32 Hour { get; set; }
        public Int32 Minute { get; set; }
        public Int32 Second { get; set; }
        public Timer TimerInstance { get; private set; }

        /// <summary>
        /// 実行されるタスク
        /// </summary>
        /// <param name="sender">イベントセンダ</param>
        public abstract void run(Object sender);

        /// <summary>
        /// タイマをスタートする
        /// </summary>
        public void start()
        {
            TimerInstance = TimerUtil.DailyTimer(run, Hour, Minute, Second);
        }

        /// <summary>
        /// タイマを停止しリソースを破棄する
        /// </summary>
        public void stop()
        {
            TimerInstance.Dispose();
            TimerInstance = null;
        }
    }
}
