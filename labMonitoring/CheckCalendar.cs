using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DDay.iCal;

namespace LabMonitoring
{
    /// <summary>
    /// カレンダのイベントチェックを行う日次タスク
    /// </summary>
    public class CheckCalendar : DailyTask
    {
        const string bottiPost = "【WARNING】 ぼっち飯警報発令！ 速やかに食事の準備を始めましょう";
        /// <summary>
        /// チェック対象のカレンダ
        /// </summary>
        public readonly Dictionary<string, string> TargetCalendar;
        private List<Timer> calTimer;
        private Timer bottiTimer;

        /// <summary>
        /// ログ出力先を指定して <b>CheckCalendar</b> を初期化する
        /// </summary>
        /// <param name="output">ログ出力デリゲート</param>
        public CheckCalendar(logOutput output)
        {
            LogOutput = output;
            TargetCalendar = new Dictionary<string, string>(3);
            TargetCalendar.Add(
                // ubi-lab
                "https://www.google.com/calendar/ical/o9maiadqkkharfiulir7t7b02o%40group.calendar.google.com/private-86ed854da9c66146539f063f73018231/basic.ics",
                "学会や出張など");
            TargetCalendar.Add(
                // ubi-meeting
                "https://www.google.com/calendar/ical/0igfeooghukogkuv32k0tinp58%40group.calendar.google.com/private-f839f24192ba67940b02dad0e3ce3a22/basic.ics",
                "ミーティング");
            TargetCalendar.Add(
                // ubi-event
                "https://www.google.com/calendar/ical/ubi-license%40is.naist.jp/private-b105373c049e97ad17f71606354d047b/basic.ics",
                "研究室内イベント");
            TargetCalendar.Add(
                // ito-lab meeting
                "https://www.google.com/calendar/ical/ito-license%40is.naist.jp/private-d465374335b22e44df3c0a58e9aa4d4c/basic.ics",
                "伊藤研ミーティング");

            calTimer = new List<System.Threading.Timer>();
            Hour = 7;
            run(new object());
        }

        /// <see cref="LabMonitoring.DailyTask"/>
        public override void run(Object sender)
        {
            /* Clean list */
            foreach (Timer t in calTimer) {
                if (t != null) t.Dispose();
            }
            if (bottiTimer != null) bottiTimer.Dispose();
            calTimer.Clear();

            bool bottiFlag = true;
            StringBuilder sb = new StringBuilder(140);
            sb.Append("[本日の予定] 今日は ");

            foreach (KeyValuePair<string, string> kvp in TargetCalendar)
            {
                try
                {
                    IICalendarCollection cal = iCalendar.LoadFromUri(new Uri(kvp.Key));
                    IList<Occurrence> events = cal.GetOccurrences(DateTime.Today, DateTime.Today.AddDays(1));

                    foreach (Occurrence ev in events)
                    {
                        Event rc = ev.Source as Event;
                        StringBuilder sbb = new StringBuilder(140);
                        sbb.Append("[次の予定] ").Append(rc.Summary);

                        if (!string.IsNullOrEmpty(rc.Location))
                        {
                            sbb.Append(" at ").Append(rc.Location);
                        }
                        sbb.Append(" (");

                        DateTime postTime;
                        if (rc.IsAllDay)
                        {
                            if (!rc.Start.AddDays(1).Equals(DateTime.Today)) continue;
                            /* All day event */
                            sbb.Append("終日");
                            postTime = DateTime.Now.AddMinutes(3);
                        }
                        else
                        {
                            sbb.Append(rc.Start.Local.ToShortTimeString()).Append(" - ").Append(rc.End.Local.ToShortTimeString());
                            DateTime start = rc.Start.Local.AddMinutes(-15);
                            postTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 
                                start.Hour, start.Minute - 15, start.Second);
                        }
                        sbb.Append(")");
                        calTimer.Add(TimerUtil.OnceTimer((x) => { Twitter.GetInstance().StatusUpdate(sbb.ToString()); }, postTime));

                        /* ボッチ飯警報 */
                        if (bottiFlag && rc.Summary.Contains("江藤"))
                        {
                            bottiFlag = false;
                        }
                    }

                    if (events.Count > 0)
                    {
                        sb.Append(kvp.Value).Append("の予定が").Append(events.Count).Append("件, ");
                    }
                }
                catch (Exception ex)
                {
                    Log("Calendar load failed: " + ex.ToString());
                }
            }

            string post = sb.ToString();
            int lastCamma = post.LastIndexOf(", ");
            if (lastCamma != -1)
            {
                post = post.Substring(0, lastCamma) + "あります．";
            }
            else
            {
                post = "[今日の予定] 今日は予定はありません．";
            }

            if (sender == null)
            {
                Log("CalendarTweet: " + post);
                Twitter.GetInstance().StatusUpdate(post);
            }

            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) bottiFlag = false;

            if (bottiFlag)
            {
                bottiTimer = TimerUtil.OnceTimer((x) => { Twitter.GetInstance().StatusUpdate(bottiPost); }, DateTime.Today.AddHours(12).AddMinutes(20));
            }
        }
    }
}
