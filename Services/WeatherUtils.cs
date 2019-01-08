using AppWeather.Controllers.CurrentData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace AppWeather.Services
{
    public class WeatherUtils
    {
        // 取的即刻時間填入updateTime變數中，以傳到WeatherController.cs中的Index()函式內
        public static DateTime updateTime;

        // 取的JSON資料填入weatherData物件中，以傳到WeatherController.cs中的Index()函式內
        public static WeatherDataInJson weatherData;

        public static void exeGetJasonByUrl()
        {
            while (1 == 1)
            {
                DateTime currentTime = DateTime.Now;
                updateTime = currentTime;

                String format_Year_Month_Date = "yyyy-MM-dd";

                weatherData = new WeatherDataInJson();

                try
                {
                    String url_currentData = "http://opendata.cwb.gov.tw/api/v1/rest/datastore/O-A0001-001?Authorization=$Authorization_Key&locationName=%E6%96%87%E5%B1%B1";
                    String url_twoDaysWeather =
                    "http://opendata.cwb.gov.tw/api/v1/rest/datastore/F-D0047-061?" +
                    "Authorization=$Authorization_Key" +
                    "&locationName=%E6%96%87%E5%B1%B1%E5%8D%80" +
                    "&sort=time";                  

                    String url_oneWeekWeather_forToday =
                    "http://opendata.cwb.gov.tw/api/v1/rest/datastore/F-D0047-063?" +
                    "Authorization=$Authorization_Key" +
                    "&locationName=%E6%96%87%E5%B1%B1%E5%8D%80" +
                    "&sort=time";
                    //http://opendata.cwb.gov.tw/api/v1/rest/datastore/F-D0047-063?Authorization=$Authorization_Key&locationName=%E6%96%87%E5%B1%B1%E5%8D%80&sort=time

                    String url_oneWeekWeather =
                    "http://opendata.cwb.gov.tw/api/v1/rest/datastore/F-D0047-063?" +
                    "Authorization=$Authorization_Key" +
                    "&locationName=%E6%96%87%E5%B1%B1%E5%8D%80" +
                    "&sort=time" +
                    "&startTime="
                    + currentTime.AddDays(1).ToString(format_Year_Month_Date) + "T06:00:00,"
                    + currentTime.AddDays(2).ToString(format_Year_Month_Date) + "T06:00:00,"
                    + currentTime.AddDays(3).ToString(format_Year_Month_Date) + "T06:00:00,"
                    + currentTime.AddDays(4).ToString(format_Year_Month_Date) + "T06:00:00,"
                    + currentTime.AddDays(5).ToString(format_Year_Month_Date) + "T06:00:00,"
                    + currentTime.AddDays(6).ToString(format_Year_Month_Date) + "T06:00:00,"
                    + currentTime.AddDays(7).ToString(format_Year_Month_Date) + "T06:00:00";
           
                    //String url_PM25 = "http://opendata.epa.gov.tw/ws/Data/ATM00625/?$format_Year_Month_Date=json";
                    String url_AQI = "http://opendata2.epa.gov.tw/AQI.json";

                    weatherData.jsonStr_currentData = getJsonByUrl(url_currentData);
                    weatherData.jsonStr_twoDaysWeather = getJsonByUrl(url_twoDaysWeather);
                    weatherData.jsonStr_oneWeekWeather = getJsonByUrl(url_oneWeekWeather);
                    weatherData.jsonStr_oneWeekWeather_forToday = getJsonByUrl(url_oneWeekWeather_forToday);
                    weatherData.jsonStr_AQI = getJsonByUrl(url_AQI);


                    System.Threading.Thread.Sleep(1000 * 60 * 30);
                }

                catch (Exception ex)
                {
                    Console.Write("錯誤.." + ex.Message);
                }
             
            }
        }


        public static String getJsonByUrl(String url)
        {
            String re = "";

            WebClient client_oneWeekWeather = new WebClient();
            client_oneWeekWeather.Encoding = Encoding.UTF8; // 設定Webclient.Encoding
            re = client_oneWeekWeather.DownloadString(url);
            return re;
        }
    }


    public class WeatherDataInJson
    {
        public string jsonStr_currentData { set; get; }
        public string jsonStr_twoDaysWeather { set; get; }
        public string jsonStr_oneWeekWeather { set; get; }
        public string jsonStr_oneWeekWeather_forToday { set; get; }
        public string jsonStr_AQI { set; get; }
    }
}