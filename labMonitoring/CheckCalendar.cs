using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DDay.iCal;

namespace LabMonitoring
{
    class CheckCalendar
    {
        const string bottiPost = "【WARNING】 ぼっち飯警報発令！ 速やかに食事の準備を始めましょう";
        public readonly Dictionary<string, string> TargetCalendar;
        public logOutput log;
        private Timer calUpdateTimer;
        private List<Timer> calTimer;
        private Timer bottiTimer;

        public CheckCalendar(logOutput output)
        {
            log = output;
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

            calTimer = new List<System.Threading.Timer>();
            calUpdateTimer = TimerUtil.DailyTimer(GetCalEvents, 7);
        }

        private void GetCalEvents(Object sender)
        {
            /* Clean list */
            foreach (Timer t in calTimer) {
                t.Dispose();
            }
            bottiTimer.Dispose();
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

                        if (rc.Location != "")
                        {
                            sbb.Append(" at ").Append(rc.Location);
                        }
                        sbb.Append(" (");

                        DateTime postTime;
                        if (rc.IsAllDay)
                        {
                            /* All day event */
                            sbb.Append("終日");
                            postTime = DateTime.Now.AddMinutes(3);
                        }
                        else
                        {
                            sbb.Append(rc.Start.Local.ToShortTimeString()).Append(" - ").Append(rc.End.Local.ToShortTimeString());
                            postTime = rc.Start.Local.AddMinutes(-15);
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
                    log("Calendar load failed: " + ex.ToString());
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

            Twitter.GetInstance().StatusUpdate(post);

            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) bottiFlag = false;

            if (bottiFlag)
            {
                bottiTimer = TimerUtil.OnceTimer((x) => { Twitter.GetInstance().StatusUpdate(bottiPost); }, DateTime.Today.AddHours(12).AddMinutes(20));
            }
        }
    }
}
