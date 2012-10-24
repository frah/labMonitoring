using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LabMonitoring
{
    [Serializable]
    public class KamatteSettings
    {
        public int WaitTime { get; set; }
        public List<TargetUser> Targets { get; set; }

        public List<string> GetTargetIdArray()
        {
            var ret = new List<string>();
            foreach (var t in Targets)
            {
                ret.Add(t.Id.ToString());
            }
            return ret;
        }
        public List<string> GetTargetNameArray()
        {
            var ret = new List<string>();
            foreach (var t in Targets)
            {
                ret.Add(t.Name);
            }
            return ret;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("WaitTime: "+WaitTime);
            sb.AppendLine("Targets:");
            foreach (var t in Targets) {
                sb.AppendLine(t.ToString());
            }
            return sb.ToString();
        }
    }

    public class TargetUser
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int TotalKamatteCount { get; set; }
        public int DailyKamatteCount { get; set; }
        public string Filter { get; set; }

        public override string ToString()
        {
            return Name + " (" + Id + "): " + DailyKamatteCount + "/" + TotalKamatteCount + "[" + Filter + "]";
        }
    }
}
