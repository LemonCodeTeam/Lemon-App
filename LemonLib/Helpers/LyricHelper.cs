using System;
using System.Collections.Generic;
using System.Text;

namespace LemonLib
{
    public class LyricHelper
    {
        public static String[] parserLine(String str, List<String> times, List<String> texs, Dictionary<String, String> data, bool doesAdd = true)
        {
            if (!str.StartsWith("[ti:") && !str.StartsWith("[ar:") && !str.StartsWith("[al:") && !str.StartsWith("[by:") && !str.StartsWith("[offset:") && !str.StartsWith("[kana") && str.Length != 0)
            {
                String TimeData = TextHelper.XtoYGetTo(str, "[", "]", 0);
                String io = "[" + TimeData + "]";
                String TexsData = str.Replace(io, "");
                //String unTimeData = TimeData.Substring(0, TimeData.Length - 1);
                if (doesAdd)
                {
                    if (data.ContainsKey(TimeData))
                    {
                        texs.Add(TexsData);
                        data[TimeData] += "^" + TexsData;
                    }
                    else
                    {
                        times.Add(TimeData);
                        texs.Add(TexsData);
                        data.Add(TimeData, TexsData);
                    }
                }
                return new string[2] { TimeData, TexsData };
            }
            else return null;
        }

        public static string YwY(string str, int i)
        {//00:02.06 => 00:02.07
            string lstr = TextHelper.XtoYGetTo(str + "]", ".", "]", 0);//06
            string LastTime = (int.Parse(lstr) + i).ToString();//06+i
            if (LastTime.Length == 1)
                LastTime = "0" + LastTime;
            return str.Replace(lstr, LastTime.ToString());
        }
    }
}
