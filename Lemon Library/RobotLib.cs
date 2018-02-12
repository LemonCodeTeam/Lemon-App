using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonLibrary
{
    public class RobotLib
    {
        public async Task<string[]> TalkAsync(string text,string id)
        {
            try
            {
                JObject obj = JObject.Parse(await HttpHelper.GetWebAsync("http://www.tuling123.com/openapi/api?key=0651b32a3a6c8f54c7869b9e62872796&info=" + Uri.EscapeUriString(text) + "&userid=" + Uri.EscapeUriString(id)));
                if ((string)obj["code"] == "100000" || obj["code"].ToString() == "40002")
                    return new string[2] { obj["text"].ToString(), "null" };
                else if ((string)obj["code"] == "200000")
                    return new string[2] { obj["text"].ToString(), obj["url"].ToString() };
                else return new string[2] { "", "" };
            }
            catch { return new string[2] { "小萌机器人似乎遇到了些问题","null" }; }
        }
    }
}
