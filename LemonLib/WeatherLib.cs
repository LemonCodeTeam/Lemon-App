using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonLib
{
    public class WeatherLib
    {
        public async Task<InfoHelper.Weather> GetWeatherAsync(string i)
        {
            InfoHelper.Weather dt = new InfoHelper.Weather();
            JObject p = JObject.Parse(await HttpHelper.GetWebAsync($"https://route.showapi.com/104-29?showapi_sign=cfa206656db244c089be2d1499735bb5&showapi_appid=29086&city={Uri.EscapeUriString(i)}"));
            dt.KongQiZhiLiang = p["showapi_res_body"]["pm"]["quality"] + "  " + p["showapi_res_body"]["pm"]["aqi"];

            JObject obj = JObject.Parse(await HttpHelper.GetWebAsync($"https://free-api.heweather.com/v5/now?city={Uri.EscapeUriString(i)}&key=f97e6a6ad4cd49babd0538747c86b88d"));
            dt.TiGan = "体感温度:" + obj["HeWeather5"][0]["now"]["fl"] + "°";
            dt.FenSu= obj["HeWeather5"][0]["now"]["wind"]["dir"] + "    " + obj["HeWeather5"][0]["now"]["wind"]["sc"] + "级";
            dt.MiaoShu = obj["HeWeather5"][0]["now"]["cond"]["txt"].ToString();
            dt.NenJianDu= "能见度:" + obj["HeWeather5"][0]["now"]["vis"];
            dt.Qiwen= obj["HeWeather5"][0]["now"]["tmp"] + "°";

            JObject obj1 = JObject.Parse(await HttpHelper.GetWebAsync($"https://free-api.heweather.com/v5/forecast?city={Uri.EscapeUriString(i)}&key=f97e6a6ad4cd49babd0538747c86b88d"));
            List<InfoHelper.WeatherByDay> ddt = new List<InfoHelper.WeatherByDay>();
            ddt.Add(new InfoHelper.WeatherByDay() {Icon= $"http://files.heweather.com/cond_icon/{obj1["HeWeather5"][0]["daily_forecast"][0]["cond"]["code_d"]}.png",
            Date= obj1["HeWeather5"][0]["daily_forecast"][0]["date"].ToString(),
            QiWen= obj1["HeWeather5"][0]["daily_forecast"][0]["tmp"]["max"] + "℃-" + obj1["HeWeather5"][0]["daily_forecast"][0]["tmp"]["min"] + "°"});
            ddt.Add(new InfoHelper.WeatherByDay()
            {
                Icon = $"http://files.heweather.com/cond_icon/{obj1["HeWeather5"][0]["daily_forecast"][1]["cond"]["code_d"]}.png",
                Date = obj1["HeWeather5"][0]["daily_forecast"][1]["date"].ToString(),
                QiWen = obj1["HeWeather5"][0]["daily_forecast"][1]["tmp"]["max"] + "℃-" + obj1["HeWeather5"][0]["daily_forecast"][1]["tmp"]["min"] + "°"
            });
            ddt.Add(new InfoHelper.WeatherByDay()
            {
                Icon = $"http://files.heweather.com/cond_icon/{obj1["HeWeather5"][0]["daily_forecast"][2]["cond"]["code_d"]}.png",
                Date = obj1["HeWeather5"][0]["daily_forecast"][2]["date"].ToString(),
                QiWen = obj1["HeWeather5"][0]["daily_forecast"][2]["tmp"]["max"] + "℃-" + obj1["HeWeather5"][0]["daily_forecast"][2]["tmp"]["min"] + "°"
            });
            ddt.Add(new InfoHelper.WeatherByDay()
            {
                Icon = $"http://files.heweather.com/cond_icon/{obj1["HeWeather5"][0]["daily_forecast"][3]["cond"]["code_d"]}.png",
                Date = obj1["HeWeather5"][0]["daily_forecast"][3]["date"].ToString(),
                QiWen = obj1["HeWeather5"][0]["daily_forecast"][3]["tmp"]["max"] + "℃-" + obj1["HeWeather5"][0]["daily_forecast"][3]["tmp"]["min"] + "°"
            });
            ddt.Add(new InfoHelper.WeatherByDay()
            {
                Icon = $"http://files.heweather.com/cond_icon/{obj1["HeWeather5"][0]["daily_forecast"][4]["cond"]["code_d"]}.png",
                Date = obj1["HeWeather5"][0]["daily_forecast"][4]["date"].ToString(),
                QiWen = obj1["HeWeather5"][0]["daily_forecast"][4]["tmp"]["max"] + "℃-" + obj1["HeWeather5"][0]["daily_forecast"][4]["tmp"]["min"] + "°"
            });
            ddt.Add(new InfoHelper.WeatherByDay()
            {
                Icon = $"http://files.heweather.com/cond_icon/{obj1["HeWeather5"][0]["daily_forecast"][5]["cond"]["code_d"]}.png",
                Date = obj1["HeWeather5"][0]["daily_forecast"][5]["date"].ToString(),
                QiWen = obj1["HeWeather5"][0]["daily_forecast"][5]["tmp"]["max"] + "℃-" + obj1["HeWeather5"][0]["daily_forecast"][5]["tmp"]["min"] + "°"
            });
            ddt.Add(new InfoHelper.WeatherByDay()
            {
                Icon = $"http://files.heweather.com/cond_icon/{obj1["HeWeather5"][0]["daily_forecast"][6]["cond"]["code_d"]}.png",
                Date = obj1["HeWeather5"][0]["daily_forecast"][6]["date"].ToString(),
                QiWen = obj1["HeWeather5"][0]["daily_forecast"][6]["tmp"]["max"] + "℃-" + obj1["HeWeather5"][0]["daily_forecast"][6]["tmp"]["min"] + "°"
            });
            dt.Data = ddt;
            return dt;
        }
    }
}
