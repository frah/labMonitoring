using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace LabMonitoring
{
    /// <summary>
    /// タイマーの簡易設定を目的としたラッパクラス
    /// </summary>
    public class TimerUtil : Logger
    {
        private TimerUtil() { }

        /// <summary>
        /// 開始日時を指定したタイマを設定する
        /// </summary>
        /// <param name="callback">タイマコールバック関数</param>
        /// <param name="startTime">実行開始日時</param>
        /// <param name="period">繰り返し間隔</param>
        /// <returns>生成されたタイマインスタンス</returns>
        public static Timer TimerWithStartTime(TimerCallback callback, DateTime startTime, TimeSpan period)
        {
            TimeSpan ts = startTime - DateTime.Now;
            Trace.WriteLine("[" + DateTime.Now.ToString() + "] LabMonitoring.TimerUtil#TimerWithStartTime\r\n" 
                + "New timer is set at " + startTime.ToString() + " every " + period.Minutes + " minutes.");
            return new Timer(callback, null, ts, period);
        }

        /// <summary>
        /// 1日に1度実行されるタイマを設定する
        /// 既に指定された時間を過ぎている場合は次の日の同時刻に設定する
        /// </summary>
        /// <param name="callback">タイマコールバック関数</param>
        /// <param name="hour">開始時</param>
        /// <param name="minute">開始分</param>
        /// <param name="second">開始秒</param>
        /// <returns>生成されたタイマインスタンス</returns>
        public static Timer DailyTimer(TimerCallback callback, Int32 hour = 0, Int32 minute = 0, Int32 second = 0)
        {
            DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);
            if (DateTime.Now.CompareTo(startTime) >= 0)
            {
                startTime = startTime.AddDays(1);
            }

            return TimerWithStartTime(callback, startTime, new TimeSpan(24, 0, 0));
        }

        /// <summary>
        /// 一度だけ実行されるタイマを設定する
        /// </summary>
        /// <param name="callback">タイマコールバック関数</param>
        /// <param name="startTime">開始する日時</param>
        /// <returns>生成されたタイマインスタンス</returns>
        public static Timer OnceTimer(TimerCallback callback, DateTime startTime)
        {
            TimeSpan ts = startTime - DateTime.Now;
            Trace.WriteLine("[" + DateTime.Now.ToString() + "] LabMonitoring.TimerUtil#OnceTimer\r\n"
                + "New timer is set at " + startTime.ToString() + " at once.");
            return OnceTimer(callback, (int)ts.TotalMilliseconds);
        }

        /// <summary>
        /// 一度だけ実行されるタイマを設定する
        /// </summary>
        /// <param name="callback">タイマコールバック関数</param>
        /// <param name="dueTime">開始までの時間．0で即時開始</param>
        /// <returns>生成されたタイマインスタンス</returns>
        public static Timer OnceTimer(TimerCallback callback, int dueTime)
        {
            if (dueTime < 0) return null;
            Trace.WriteLine("[" + DateTime.Now.ToString() + "] LabMonitoring.TimerUtil#OnceTimer\r\n" 
                + "New timer is set after " + dueTime + " ms at once.");
            return new Timer(callback, null, dueTime, Timeout.Infinite);
        }
    }
}
