using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace LemonLib
{
    public class RomajiHelper
    {
        public static async Task<List<string>> GetRomaji(string text) {
            string data = await HttpHelper.PostWebSiteEzlang("https://www.ezlang.net/ajax/tool_data.php","txt="+HttpUtility.UrlDecode(text.ToString())+ "&sn=romaji");
            MainClass.DebugCallBack(data,"Lyric");
            List<string> vs = new List<string>();
            string xml = JArray.Parse(data)[1].ToString();
            string doc ="<List>"+ xml+"</List>";
            XmlDocument xd = new();
            xd.LoadXml(doc);
            var root = xd.SelectSingleNode("List");
            foreach (XmlElement div in root)
            {
                string line = "";
                var spans = div.SelectNodes("span");
                foreach (XmlElement span in spans)
                {
                    if (span.InnerXml.Contains("ruby"))
                    {
                        var d = span.SelectSingleNode("ruby").SelectSingleNode("rt").InnerText;
                        line += " " + d;
                    }
                }
                vs.Add(line);
            }
            return vs;
        }
    }
}
