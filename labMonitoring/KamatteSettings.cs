using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LabMonitoring
{
    /// <summary>
    /// 誰かかまってやれよBOTの設定
    /// </summary>
    [Serializable]
    public class KamatteSettings
    {
        /// <summary>
        /// 何分間無視されたツイートを対象にするか
        /// </summary>
        public int WaitTime { get; set; }
        /// <summary>
        /// すべてのユーザを対象とするフィルタ
        /// </summary>
        public string GlobalFilter { get; set; }
        /// <summary>
        /// BOTのターゲット
        /// </summary>
        public List<TargetUser> Targets { get; set; }

        /// <summary>
        /// TargetのIdのリストを返す
        /// </summary>
        /// <returns>TargetのIdリスト</returns>
        public List<string> GetTargetIdArray()
        {
            var ret = new List<string>();
            foreach (var t in Targets)
            {
                ret.Add(t.Id.ToString());
            }
            return ret;
        }

        /// <summary>
        /// TargetのNameのリストを返す
        /// </summary>
        /// <returns>TargetのNameリスト</returns>
        public List<string> GetTargetNameArray()
        {
            var ret = new List<string>();
            foreach (var t in Targets)
            {
                ret.Add(t.Name);
            }
            return ret;
        }

        /// <summary>
        /// ユーザIDからTargetUserを得る
        /// </summary>
        /// <param name="targetId">対象ユーザID</param>
        /// <returns>指定されたユーザIDのTargetUser</returns>
        public TargetUser GetTargetUserFromId(decimal targetId)
        {
            foreach (var t in Targets)
            {
                if (t.Id == targetId) return t;
            }
            return null;
        }

        /// <summary>
        /// 指定されたユーザのツッコミ数をインクリメント
        /// </summary>
        /// <param name="targetId">対象ユーザID</param>
        /// <returns>インクリメント後のTargetUser</returns>
        public TargetUser IncrementKamatteCount(decimal targetId)
        {
            foreach (var t in Targets)
            {
                if (t.Id == targetId)
                {
                    t.DailyKamatteCount++;
                    t.TotalKamatteCount++;
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        /// 全ユーザの1日のツッコミ数を初期化する
        /// </summary>
        public void ClearDailyCount()
        {
            for (int i = Targets.Count - 1; i >= 0; i--)
            {
                Targets[i].DailyKamatteCount = 0;
            }
        }

        /// <summary>
        /// このクラスの文字列表記を返す
        /// </summary>
        /// <returns>このクラスの文字列表記</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("WaitTime: " + WaitTime);
            sb.AppendLine("GlobalFilter: " + GlobalFilter);
            sb.AppendLine("Targets:");
            foreach (var t in Targets) {
                sb.AppendLine(t.ToString());
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// ターゲットユーザクラス
    /// </summary>
    public class TargetUser
    {
        /// <summary>
        /// ターゲットのTwitterID
        /// </summary>
        public decimal Id { get; set; }
        /// <summary>
        /// ターゲットのTwitterScreenName
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// ターゲットの合計反応数
        /// </summary>
        public int TotalKamatteCount { get; set; }
        /// <summary>
        /// ターゲットの今日の反応数
        /// </summary>
        public int DailyKamatteCount { get; set; }
        /// <summary>
        /// 反応文字列フィルタ
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// このクラスの文字列表記を返す
        /// </summary>
        /// <returns>このクラスの文字列表記</returns>
        public override string ToString()
        {
            return Name + " (" + Id + "): " + DailyKamatteCount + "/" + TotalKamatteCount + "[" + Filter + "]";
        }
    }
}
