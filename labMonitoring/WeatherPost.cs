using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace LabMonitoring
{
    /// <summary>
    /// 天気情報を取得する日次タスク
    /// </summary>
    class WeatherPost : DailyTask
    {
        const string WeatherXML = "http://www.drk7.jp/weather/xml/29.xml";
        const string ClothDriedURL = "http://weather.yahoo.co.jp/weather/jp/expo/clothdried/29/6410.html";

        public WeatherPost(logOutput output = null)
        {
            LogOutput = output;
            Hour = 7;
        }

        public override void run(object sender)
        {
            string weather = "";
            string temp = "";
            float rainfall = 0;
            int cloth = 0;

            /* Get weather forecast */
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(WeatherXML);
                var areas = xml.DocumentElement.GetElementsByTagName("area");
                foreach (XmlElement e in areas)
                {
                    if (e.GetAttribute("id").Equals("北部"))
                    {
                        XmlElement info = e.GetElementsByTagName("info").Item(0) as XmlElement;
                        weather = info.GetElementsByTagName("weather")[0].InnerText;
                        temp = ((XmlElement)info.GetElementsByTagName("temperature")[0]).GetElementsByTagName("range")[0].InnerText;
                        var r = info.GetElementsByTagName("rainfallchance")[0].ChildNodes;
                        foreach (XmlNode el in r)
                        {
                            rainfall += int.Parse(el.InnerText);
                        }
                        rainfall /= 4;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }

            if (weather == "" || temp == "")
            {
                Log("Any errors occurred");
                return;
            }

            /* Get clothdried */
            WebResponse res = null;
            StreamReader reader = null;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ClothDriedURL);
                res = req.GetResponse();
                reader = new StreamReader(res.GetResponseStream());
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("<b>指数")) break;
                }

                cloth = int.Parse(Regex.Replace(line, "[^0-9]", ""));
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (res != null) res.Close();
            }

            StringBuilder sb = new StringBuilder(140);
            sb.Append("おはようございます．");
            sb.Append("今日は").Append(DateTime.Today.ToString("M月d日（dddd）")).Append("です．");
            sb.Append("本日の奈良県北部の天気は").Append(weather).Append("，");
            sb.Append("最高気温は").Append(temp).Append("℃，");
            sb.Append("降水確率は").Append((int)rainfall).Append("％，");
            sb.Append("洗濯指数は").Append(cloth).Append("です．");

            Log("WeatherTweet: " + sb.ToString());
            Twitter.GetInstance().StatusUpdate(sb.ToString());
        }
    }
}
