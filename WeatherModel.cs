using System;
using System.Net.Http;

namespace BlueStacksAssignment
{
    public enum Units
    {
        Standard,
        Metric,
        Imperial
    }

    public class WeatherModel
    {
        public int Visibility { set; get; }
        public MainParameters main { get; set; }
    }

    public class MainParameters
    {
        public float Temp { get; set; }
    }

    public class WeatherProcessor
    {
        private const string URL = "http://api.openweathermap.org/data/2.5/weather";
        private const string APPID = "fe2b4d56085d97e6005044b62be0231c";

        public static WeatherModel GetWeatherDetails(string cityName, Units units, Log log)
        {
            string url = string.Concat(URL, "?q=", cityName, "&units=", units.ToString().ToLower(), "&appid=", APPID);
            
            log.WriteLine($"Requesting response from URL: {url}", LogType.DEBUG);

            using (HttpResponseMessage response = ApiClient.httpClient.GetAsync(url).Result)
            {
                if (response.IsSuccessStatusCode)
                {
                    WeatherModel weatherModel = response.Content.ReadAsAsync<WeatherModel>().Result;

                    return weatherModel;
                }
                else if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    log.WriteLine($"{cityName} could not be found in database of WebAPI", LogType.ERROR);
                    return null;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}
