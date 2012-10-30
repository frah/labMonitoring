using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LabMonitoring
{
    class TimerUtil
    {
        private TimerUtil() { }

        public static Timer TimerWithStartTime(TimerCallback callback, DateTime startTime, TimeSpan period)
        {
            TimeSpan ts = startTime - DateTime.Now;
            return new Timer(callback, null, ts, period);
        }

        public static Timer DailyTimer(TimerCallback callback, Int32 hour = 0, Int32 minute = 0, Int32 second = 0)
        {
            DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);
            if (DateTime.Now.CompareTo(startTime) >= 0)
            {
                startTime = startTime.AddDays(1);
            }

            return TimerWithStartTime(callback, startTime, new TimeSpan(24, 0, 0));
        }

        public static Timer OnceTimer(TimerCallback callback, DateTime startTime)
        {
            TimeSpan ts = startTime - DateTime.Now;
            return new Timer(callback, null, (int)ts.TotalMilliseconds, Timeout.Infinite);
        }

        public static Timer OnceTimer(TimerCallback callback, int dueTime)
        {
            return new Timer(callback, null, dueTime, Timeout.Infinite);
        }
    }
}
