using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Text;
using AppWeather.Services;

namespace AppWeather.Controllers
{
    public class WeatherController : Controller
    {
        // GET: weather
        public ActionResult Index()
        {
            ViewBag.currentTime = WeatherUtils.updateTime.ToString("MM/dd dddd");

            //1. 將json轉成物件
            CurrentData.RootObject currentData =
            JsonConvert.DeserializeObject<CurrentData.RootObject>((String)WeatherUtils.weatherData.jsonStr_currentData);

            TwoDaysWeather.RootObject twoDaysWeather =
            JsonConvert.DeserializeObject<TwoDaysWeather.RootObject>((String)WeatherUtils.weatherData.jsonStr_twoDaysWeather);

            oneWeekWeather.RootObject oneWeekWeather =
            JsonConvert.DeserializeObject<oneWeekWeather.RootObject>((String)WeatherUtils.weatherData.jsonStr_oneWeekWeather);

            oneWeekWeather_forToday.RootObject forToday =
            JsonConvert.DeserializeObject<oneWeekWeather_forToday.RootObject>((String)WeatherUtils.weatherData.jsonStr_oneWeekWeather_forToday);

            var AQIdata_var = JsonConvert.DeserializeObject<List<AQI_data.RootObject>>((String)WeatherUtils.weatherData.jsonStr_AQI);
            AQI_data.RootObject aqi = AQIdata_var[13];
            //End of 1.


            //2. 取目前資料，為了拿到即刻溫度(currentTemp)、即刻濕度(currentHumd_float)
            //   、即刻天氣狀況(currentWeather)、即刻天氣狀況碼(currentWeatherCode)。
            //   並判斷出來的即刻濕度的意涵(currentHumdMeaning)、以及當下要顯示的背景圖片路徑(currentWeatherPictureString)
            Double currentTemp = 20.0;
            Double currentHumd_float = 0.7;
            String currentHumdMeaning = "舒適濕度";

            currentTemp = Convert.ToDouble(currentData.records.location[0].weatherElement[3].elementValue);
            currentHumd_float = Convert.ToDouble(currentData.records.location[0].weatherElement[4].elementValue);


            if (currentHumd_float > 0.7)
            {
                currentHumdMeaning = "潮溼";
            }
            else if (currentHumd_float > 0.4 && currentHumd_float <= 0.7)
            {
                currentHumdMeaning = "舒適濕度";
            }
            else if (currentHumd_float <= 0.4)
            {
                currentHumdMeaning = "乾燥";
            }
            int currentHumd = (int)(currentHumd_float * 100);

            ViewBag.currentTemp = currentTemp;
            ViewBag.currentHumd = currentHumd;
            ViewBag.currentHumdMeaning = currentHumdMeaning;

            String currentWeather;
            String currentWeatherCode, currentWeatherCodeString;
            String currentWeatherForPicture;
            String currentWeatherPictureString;

            currentWeather = twoDaysWeather.records.locations[0].location[0].weatherElement[0].time[0].elementValue;
            currentWeatherCode = twoDaysWeather.records.locations[0].location[0].weatherElement[0].time[0].parameter[0].parameterValue;
            currentWeatherForPicture = dictOfImages[currentWeatherCode];

            IEnumerable<String> lines;

            if (WeatherUtils.updateTime.Hour > 6 && WeatherUtils.updateTime.Hour < 18)
            {
                lines = System.IO.File.ReadLines(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Images/day_pics/") + "numberOfPictures.txt");
            }
            else
            {
                lines = System.IO.File.ReadLines(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Images/night_pics/") + "numberOfPictures.txt");
            }

            string[] listOfLines = lines.ToArray();         

            Dictionary<string, string> dictOfNumberOfPictures = new Dictionary<string, string>()
            {
                {"sunny", listOfLines[0]}, {"cloudy", listOfLines[1]},
                {"overcast", listOfLines[2]}, {"light_rain", listOfLines[3]},
                {"heavy_rain", listOfLines[4]}
            };

            string chosenImg = (rnd_SelectedImgNumber.Next(Convert.ToInt16(dictOfNumberOfPictures[currentWeatherForPicture])) + 1).ToString();

            if (currentWeatherCode == ""){ currentWeatherCode = "02"; }

            //From 06:00 to 18:00 we use weather icons of the day; from 18:00 to 06:00 we use weather icons of the night.
            if (WeatherUtils.updateTime.Hour > 6 && WeatherUtils.updateTime.Hour < 18)
            {
                currentWeatherCodeString = dictWeatherCodeInString_day[currentWeatherCode];
                currentWeatherPictureString = "../Content/Images/day_pics/" + currentWeatherForPicture + "/" + chosenImg + ".jpg";
            }
            else
            {
                currentWeatherCodeString = dictWeatherCodeInString_night[currentWeatherCode];
                currentWeatherPictureString = "../Content/Images/night_pics/" + currentWeatherForPicture + "/" + chosenImg + ".jpg";
            }

            ViewBag.currentWeather = currentWeather;
            ViewBag.currentWeatherCodeString = currentWeatherCodeString;
            ViewBag.currentWeatherPictureString = currentWeatherPictureString;
            //End of 2.


            //3. 取得明天天氣(weather_tomorrow)、後天天氣(weather_dayAfterTomorrow)；
            //   今天、明天、後天最高溫(maxTemp_*)，最低溫(minTemp_*)；
            //   今天白天、夜晚的降雨量(pop_today_day; pop_today_night)；
            //   今天的紫外線指數(uvi_today)，並判斷出其的意義(uvi_today_meaning);
            //   目前的空氣品質指數(currentAqi)，並判斷出其的意義(currentAqiMeaning);
            //   明後兩天的星期日期(date_tomorrow; date_dayAfterTomorrow)。

            int maxTemp_today, minTemp_today;
            int pop_today_day = 0, pop_today_night = 0;
            int uvi_today = 2;
            String uvi_today_meaning = "低量級";
            int currentAqi = 25;
            String currentAqiMeaning = "良好";


            minTemp_today = Convert.ToInt32(forToday.records.locations[0].location[0].weatherElement[7].time[0].elementValue);
            maxTemp_today = Convert.ToInt32(forToday.records.locations[0].location[0].weatherElement[11].time[0].elementValue);





            if (WeatherUtils.updateTime.Hour >= 0 && WeatherUtils.updateTime.Hour < 5)
            {
                pop_today_day = Convert.ToInt32(forToday.records.locations[0].location[0].weatherElement[0].time[1].elementValue);
                pop_today_night = Convert.ToInt32(forToday.records.locations[0].location[0].weatherElement[0].time[2].elementValue);
            }

            else if (WeatherUtils.updateTime.Hour >= 6 && WeatherUtils.updateTime.Hour < 17)
            {
                pop_today_day = Convert.ToInt32(forToday.records.locations[0].location[0].weatherElement[0].time[0].elementValue);
                pop_today_night = Convert.ToInt32(forToday.records.locations[0].location[0].weatherElement[0].time[1].elementValue);
            }


            uvi_today = Convert.ToInt32(forToday.records.locations[0].location[0].weatherElement[8].time[0].parameter[0].parameterValue);
            uvi_today_meaning = forToday.records.locations[0].location[0].weatherElement[8].time[0].parameter[1].parameterValue;

            if (aqi == null)
            {
                currentAqi = 25;
            }
            else
            {
                if (aqi.AQI == "") //if the data is empty
                {
                    currentAqi = 25;
                }
                else
                {
                    currentAqi = Convert.ToInt32(aqi.AQI);
                }
            }

            if (currentAqi <= 50)
            {
                currentAqiMeaning = "良好";
            }
            else if (currentAqi > 50 && currentAqi <= 100)
            {
                currentAqiMeaning = "普通";
            }
            else if (currentAqi > 100 && currentAqi <= 150)
            {
                currentAqiMeaning = "對敏感族群不健康";
            }
            else if (currentAqi > 150 && currentAqi <= 200)
            {
                currentAqiMeaning = "對所有族群不健康";
            }
            else if (currentAqi > 200 && currentAqi <= 300)
            {
                currentAqiMeaning = "非常不健康";
            }
            else if (currentAqi > 300)
            {
                currentAqiMeaning = "危害";
            }


            //WeatherPrediction Section
            Weatherprediction[] weatherPrediction = new Weatherprediction[6];
            for (int i = 0; i < 6; i++)
            {
                weatherPrediction[i] = new Weatherprediction();
            }

            //default
            weatherPrediction[0].date = "二";
            weatherPrediction[1].date = "三";
            weatherPrediction[2].date = "四";
            weatherPrediction[3].date = "五";
            weatherPrediction[4].date = "六";
            weatherPrediction[5].date = "日";

            //default
            foreach (Weatherprediction prediction in weatherPrediction)
            {
                prediction.weather = "多雲";
                prediction.weatherCode = "02";
                prediction.minTemp = "12";
                prediction.maxTemp = "20";
                prediction.weatherCodeString = "wi wi-day-cloudy";
            }


            //get data all predictions of 6 days
            if (WeatherUtils.updateTime.Hour >= 0 && WeatherUtils.updateTime.Hour < 23)
            {
                for (int i = 0; i < 6; i++)
                {
                    weatherPrediction[i].date = WeatherUtils.updateTime.AddDays(i + 1).ToString("dddd").Substring(2, 1);
                    weatherPrediction[i].weather = oneWeekWeather.records.locations[0].location[0].weatherElement[5].time[i].elementValue;
                    weatherPrediction[i].weatherCode = oneWeekWeather.records.locations[0].location[0].weatherElement[5].time[i].parameter[0].parameterValue;
                    if (weatherPrediction[i].weatherCode == "") { weatherPrediction[i].weatherCode = "02"; }
                    weatherPrediction[i].weatherCodeString = dictWeatherCodeInString_day[weatherPrediction[i].weatherCode];
                    weatherPrediction[i].minTemp = oneWeekWeather.records.locations[0].location[0].weatherElement[7].time[i].elementValue;
                    weatherPrediction[i].maxTemp = oneWeekWeather.records.locations[0].location[0].weatherElement[11].time[i].elementValue;
                }
            }
            //End of WeatherPrediction Section


            ViewBag.minTemp_today = minTemp_today;
            ViewBag.maxTemp_today = maxTemp_today;

            ViewBag.pop_today_day = pop_today_day;
            ViewBag.pop_today_night = pop_today_night;
            ViewBag.uvi_today = uvi_today;
            ViewBag.uvi_today_meaning = uvi_today_meaning;
            ViewBag.currentAqi = currentAqi;
            ViewBag.currentAqiMeaning = currentAqiMeaning;

            ViewBag.date_plus1 = weatherPrediction[0].date;
            ViewBag.date_plus2 = weatherPrediction[1].date;
            ViewBag.date_plus3 = weatherPrediction[2].date;
            ViewBag.date_plus4 = weatherPrediction[3].date;
            ViewBag.date_plus5 = weatherPrediction[4].date;
            ViewBag.date_plus6 = weatherPrediction[5].date;

            ViewBag.weather_plus1 = weatherPrediction[0].weather;
            ViewBag.weather_plus2 = weatherPrediction[1].weather;
            ViewBag.weather_plus3 = weatherPrediction[2].weather;
            ViewBag.weather_plus4 = weatherPrediction[3].weather;
            ViewBag.weather_plus5 = weatherPrediction[4].weather;
            ViewBag.weather_plus6 = weatherPrediction[5].weather;

            ViewBag.weatherCodeString_plus1 = weatherPrediction[0].weatherCodeString;
            ViewBag.weatherCodeString_plus2 = weatherPrediction[1].weatherCodeString;
            ViewBag.weatherCodeString_plus3 = weatherPrediction[2].weatherCodeString;
            ViewBag.weatherCodeString_plus4 = weatherPrediction[3].weatherCodeString;
            ViewBag.weatherCodeString_plus5 = weatherPrediction[4].weatherCodeString;
            ViewBag.weatherCodeString_plus6 = weatherPrediction[5].weatherCodeString;

            ViewBag.minTemp_plus1 = weatherPrediction[0].minTemp;
            ViewBag.minTemp_plus2 = weatherPrediction[1].minTemp;
            ViewBag.minTemp_plus3 = weatherPrediction[2].minTemp;
            ViewBag.minTemp_plus4 = weatherPrediction[3].minTemp;
            ViewBag.minTemp_plus5 = weatherPrediction[4].minTemp;
            ViewBag.minTemp_plus6 = weatherPrediction[5].minTemp;

            ViewBag.maxTemp_plus1 = weatherPrediction[0].maxTemp;
            ViewBag.maxTemp_plus2 = weatherPrediction[1].maxTemp;
            ViewBag.maxTemp_plus3 = weatherPrediction[2].maxTemp;
            ViewBag.maxTemp_plus4 = weatherPrediction[3].maxTemp;
            ViewBag.maxTemp_plus5 = weatherPrediction[4].maxTemp;
            ViewBag.maxTemp_plus6 = weatherPrediction[5].maxTemp;
            //End of 3.


            ViewBag.cautionStatement = getPossibleCautionStatement(pop_today_day, pop_today_night, minTemp_today, maxTemp_today, currentAqi, uvi_today);

            return View();
        }

        //TEST
        //public List<WeatherDbPro> getAllDataList()
        //{
        //    List<WeatherDbPro> re = new List<WeatherDbPro>();
        //    return re;
        //}
        //static Random rnd_img_sunny = new Random();
        //static Random rnd_img_cloudy = new Random();
        //static Random rnd_img_overcast = new Random();
        //static Random rnd_img_light_rain = new Random();
        //static Random rnd_img_heavy_rain = new Random();
        static Random rnd_SelectedImgNumber = new Random();

        static Random rnd_caution = new Random();
        public string getPossibleCautionStatement(int pop_day, int pop_night, int temp_min, int temp_max, int aqi, int uvi)
        {
            //TEST 
            //List<WeatherDbPro> weathers = getAllDataList();

            //foreach(WeatherDbPro weather in weathers )
            //{
            //    //temp_min, int temp_max
            //    if( weather.tmp_s <= temp_min )
            //    {

            //    }

            //}

            List<string> possibleCaution = new List<string>();
            List<string> cautionTmp = new List<string>(caution_pop_1);

            cautionTmp = new List<string>(caution_default);
            possibleCaution.AddRange(cautionTmp);

            if (pop_day > 0)
            {
                cautionTmp = new List<string>(caution_pop_1);
                possibleCaution.AddRange(cautionTmp);
            }
            if (pop_day == 0 && pop_night > 0)
            {
                cautionTmp = new List<string>(caution_pop_2);
                possibleCaution.AddRange(cautionTmp);
            }
            if (pop_day == 0 && pop_night > 0)
            {
                cautionTmp = new List<string>(caution_pop_3);
                possibleCaution.AddRange(cautionTmp);
            }
            if (temp_min < 15)
            {
                cautionTmp = new List<string>(caution_temp_1);
                possibleCaution.AddRange(cautionTmp);
            }
            if (temp_max > 35)
            {
                cautionTmp = new List<string>(caution_temp_2);
                possibleCaution.AddRange(cautionTmp);
            }
            if (uvi > 6)
            {
                cautionTmp = new List<string>(caution_uvi_1);
                possibleCaution.AddRange(cautionTmp);
            }
            if (aqi < 50 && pop_day == 0 && pop_night == 0)
            {
                cautionTmp = new List<string>(caution_aqi_1);
                possibleCaution.AddRange(cautionTmp);
            }
            if (aqi < 50)
            {
                cautionTmp = new List<string>(caution_aqi_2);
                possibleCaution.AddRange(cautionTmp);
            }


            int r = rnd_caution.Next(possibleCaution.Count);
            return (string)possibleCaution[r];
        }


        public string[] caution_pop_1 = new string[] { "今天會下雨，帶把傘別淋濕了！",                                                       
                                                       "如果你在想今天要不要帶傘，就帶吧！",
                                                       "良言一句三冬暖，良言二句帶把傘。"};

        public string[] caution_pop_2 = new string[] { "晚上會下雨，帶把傘別淋濕了！"};

        public string[] caution_pop_3 = new string[] { "今天好天氣！" };

        public string[] caution_temp_1 = new string[] { "今天天氣較寒冷，多加件衣服別感冒了！",
                                                        "今天氣溫低，出門可以多加件外套喔！",
                                                        "今天氣溫低，多注意保暖喔！"};

        public string[] caution_temp_2 = new string[] { "今天紫外線強，室外活動注意防曬！" };

        public string[] caution_uvi_1 = new string[] { "今天天氣炎熱，多補充水費別中暑了！",
                                                        "今天氣溫高，多注意水分的補充喔！"};

        public string[] caution_aqi_1 = new string[] { "到戶外走走享受良好的天氣吧！",
                                                       "今天好天氣，到戶外透透氣吧！"};

        public string[] caution_aqi_2 = new string[] { "現在空氣良好，到戶外透透氣吧！",
                                                       "到戶外享受新鮮空氣吧！"};

        public string[] caution_default = new string[] { "我愛台灣，台灣愛我" };

        Dictionary<string, string> dictOfImages = new Dictionary<string, string>()
        {
            {"01", "sunny"},
            {"02", "cloudy"},
            {"03", "overcast"},
            {"04", "overcast"},
            {"05", "cloudy"},
            {"06", "overcast"},
            {"07", "cloudy"},
            {"08", "cloudy"},
            {"12", "light_rain"},
            {"13", "heavy_rain"},
            {"17", "heavy_rain"},
            {"18", "heavy_rain"},
            {"24", "heavy_rain"},
            {"26", "light_rain"},
            {"31", "heavy_rain"},
            {"34", "heavy_rain"},
            {"36", "heavy_rain"},
            {"43", "sunny"},
            {"44", "cloudy"},
            {"45", "cloudy"},
            {"46", "sunny"},
            {"49", "light_rain"},
            {"57", "light_rain"},
            {"58", "heavy_rain"},
            {"59", "heavy_rain"},
            {"60", "light_rain"}
        };

        Dictionary<string, string> dictWeatherCodeInString_day = new Dictionary<string, string>()
        {
            {"01", "wi wi-day-sunny"},
            {"02", "wi wi-cloud"},
            {"03", "wi wi-cloudy"},
            {"04", "wi wi-rain"},
            {"05", "wi wi-cloudy"},
            {"06", "wi wi-cloudy"},
            {"07", "wi wi-day-cloudy"},
            {"08", "wi wi-day-cloudy"},
            {"12", "wi wi-day-showers"},
            {"13", "wi wi-day-showers"},
            {"17", "wi wi-day-storm-showers"},
            {"18", "wi wi-day-storm-showers"},
            {"24", "wi wi-day-showers"},
            {"26", "wi wi-day-rain"},
            {"31", "wi wi-day-thunderstorm"},
            {"34", "wi wi-day-storm-showers"},
            {"36", "wi wi-day-thunderstorm"},
            {"43", "wi wi-day-fog"},
            {"44", "wi wi-fog"},
            {"45", "wi wi-day-fog"},
            {"46", "wi wi-day-fog"},
            {"49", "wi wi-day-showers"},
            {"57", "wi wi-day-rain"},
            {"58", "wi wi-day-storm-showers"},
            {"59", "wi wi-day-thunderstorm"},
            {"60", "wi wi-day-snow-wind"}
        };

        Dictionary<string, string> dictWeatherCodeInString_night = new Dictionary<string, string>()
        {
            {"01", "wi wi-night-clear"},
            {"02", "wi wi-cloud"},
            {"03", "wi wi-cloudy"},
            {"04", "wi wi-rain"},
            {"05", "wi wi-cloudy"},
            {"06", "wi wi-cloudy"},
            {"07", "wi wi-night-cloudy"},
            {"08", "wi wi-night-partly-cloudy"},
            {"12", "wi wi-night-showers"},
            {"13", "wi wi-night-showers"},
            {"17", "wi wi-night-alt-storm-showers"},
            {"18", "wi wi-night-alt-storm-showers"},
            {"24", "wi wi-night-showers"},
            {"26", "wi wi-night-rain"},
            {"31", "wi wi-night-thunderstorm"},
            {"34", "wi wi-night-storm-showers"},
            {"36", "wi wi-night-thunderstorm"},
            {"43", "wi wi-night-fog"},
            {"44", "wi wi-fog"},
            {"45", "wi wi-night-fog"},
            {"46", "wi wi-night-fog"},
            {"49", "wi wi-night-showers"},
            {"57", "wi wi-night-rain"},
            {"58", "wi wi-night-storm-showers"},
            {"59", "wi wi-night-thunderstorm"},
            {"60", "wi wi-night-snow-wind"}
        };



    }//end of WeatherController

    //public class WeatherDbPro
    //{
    //    public int tmp_s = 0;
    //    public int tmp_e = 0;

    //    public int pop_s = 0;
    //    public int pop_e = 0;

    //    public String msg = "";
    //}
    public class Weatherprediction
    {
        //public string date = "";
        //public string weatherCode = "";
        //public string weatherCodeString = "";
        //public string weather = "";
        //public string minTemp = "";
        //public string maxTemp = "";
        public string date { set; get; }
        public string weatherCode { set; get; }
        public string weatherCodeString { set; get; }
        public string weather { set; get; }
        public string minTemp { set; get; }
        public string maxTemp { set; get; }
    }

    namespace CurrentData
    {
        public class Field
        {
            public string id { get; set; }
            public string type { get; set; }
        }

        public class Result
        {
            public string resource_id { get; set; }
            public List<Field> fields { get; set; }
        }

        public class Time
        {
            public string obsTime { get; set; }
        }

        public class WeatherElement
        {
            public string elementName { get; set; }
            public string elementValue { get; set; }
        }

        public class Parameter
        {
            public string parameterName { get; set; }
            public string parameterValue { get; set; }
        }

        public class Location
        {
            public string lat { get; set; }
            public string lon { get; set; }
            public string locationName { get; set; }
            public string stationId { get; set; }
            public Time time { get; set; }
            public List<WeatherElement> weatherElement { get; set; }
            public List<Parameter> parameter { get; set; }
        }

        public class Records
        {
            public List<Location> location { get; set; }
        }

        public class RootObject
        {
            public string success { get; set; }
            public Result result { get; set; }
            public Records records { get; set; }
        }
    }
    namespace TwoDaysWeather
    {

        public class Field
        {
            public string id { get; set; }
            public string type { get; set; }
        }

        public class Result
        {
            public string resource_id { get; set; }
            public List<Field> fields { get; set; }
        }

        public class Parameter
        {
            public string parameterName { get; set; }
            public string parameterValue { get; set; }
            public string parameterUnit { get; set; }
        }

        public class Time
        {
            public string startTime { get; set; }
            public string endTime { get; set; }
            public string elementValue { get; set; }
            public List<Parameter> parameter { get; set; }
            public string dataTime { get; set; }
        }

        public class WeatherElement
        {
            public string elementName { get; set; }
            public List<Time> time { get; set; }
            public string elementMeasure { get; set; }
        }

        public class Location2
        {
            public string locationName { get; set; }
            public string geocode { get; set; }
            public string lat { get; set; }
            public string lon { get; set; }
            public List<WeatherElement> weatherElement { get; set; }
        }

        public class Location
        {
            public string datasetDescription { get; set; }
            public string locationsName { get; set; }
            public string dataid { get; set; }
            public List<Location2> location { get; set; }
        }

        public class Records
        {
            public string contentDescription { get; set; }
            public List<Location> locations { get; set; }
        }

        public class RootObject
        {
            public string success { get; set; }
            public Result result { get; set; }
            public Records records { get; set; }
        }

    }
    namespace oneWeekWeather_forToday
    {
        public class Field
        {
            public string id { get; set; }
            public string type { get; set; }
        }

        public class Result
        {
            public string resource_id { get; set; }
            public List<Field> fields { get; set; }
        }

        public class Parameter
        {
            public string parameterName { get; set; }
            public string parameterValue { get; set; }
            public string parameterUnit { get; set; }
        }

        public class Time
        {
            public string startTime { get; set; }
            public string endTime { get; set; }
            public string elementValue { get; set; }
            public List<Parameter> parameter { get; set; }
        }

        public class WeatherElement
        {
            public string elementName { get; set; }
            public string elementMeasure { get; set; }
            public List<Time> time { get; set; }
        }

        public class Location2
        {
            public string locationName { get; set; }
            public string geocode { get; set; }
            public string lat { get; set; }
            public string lon { get; set; }
            public List<WeatherElement> weatherElement { get; set; }
        }

        public class Location
        {
            public string datasetDescription { get; set; }
            public string locationsName { get; set; }
            public string dataid { get; set; }
            public List<Location2> location { get; set; }
        }

        public class Records
        {
            public string contentDescription { get; set; }
            public List<Location> locations { get; set; }
        }

        public class RootObject
        {
            public string success { get; set; }
            public Result result { get; set; }
            public Records records { get; set; }
        }
    }
    namespace oneWeekWeather
    {

        public class Field
        {
            public string id { get; set; }
            public string type { get; set; }
        }

        public class Result
        {
            public string resource_id { get; set; }
            public List<Field> fields { get; set; }
        }

        public class Parameter
        {
            public string parameterName { get; set; }
            public string parameterValue { get; set; }
        }

        public class Time
        {
            public string startTime { get; set; }
            public string endTime { get; set; }
            public string elementValue { get; set; }
            public List<Parameter> parameter { get; set; }
        }

        public class WeatherElement
        {
            public string elementName { get; set; }
            public List<Time> time { get; set; }
            public string elementMeasure { get; set; }
        }

        public class Location2
        {
            public string locationName { get; set; }
            public string geocode { get; set; }
            public string lat { get; set; }
            public string lon { get; set; }
            public List<WeatherElement> weatherElement { get; set; }
        }

        public class Location
        {
            public string datasetDescription { get; set; }
            public string locationsName { get; set; }
            public string dataid { get; set; }
            public List<Location2> location { get; set; }
        }

        public class Records
        {
            public string contentDescription { get; set; }
            public List<Location> locations { get; set; }
        }

        public class RootObject
        {
            public string success { get; set; }
            public Result result { get; set; }
            public Records records { get; set; }
        }

    }
    namespace AQI_data
    {
        public class RootObject
        {
            public string AQI { get; set; }
            public string CO { get; set; }
            public string CO_8hr { get; set; }
            public string County { get; set; }
            public string NO { get; set; }
            public string NO2 { get; set; }
            public string NOx { get; set; }
            public string O3 { get; set; }
            public string O3_8hr { get; set; }
            public string PM10 { get; set; }
            public string PM10_AVG { get; set; }
            public string PM25 { get; set; }
            public string PM25_AVG { get; set; }
            public string Pollutant { get; set; }
            public string PublishTime { get; set; }
            public string SiteName { get; set; }
            public string SO2 { get; set; }
            public string Status { get; set; }
            public string WindDirec { get; set; }
            public string WindSpeed { get; set; }

        }
    }

}
