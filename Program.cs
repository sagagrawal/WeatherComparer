using Newtonsoft.Json;
using System;
using System.IO;

namespace BlueStacksAssignment
{
    class Program
    {
        public static void Main(string[] args)
        {
            Log log = null;
            JsonInputData items = null;
            try
            {
                log = new Log("Logs.txt", false);

                using (StreamReader sr = new StreamReader("input.json"))
                {
                    string json = sr.ReadToEnd();
                    items = JsonConvert.DeserializeObject<JsonInputData>(json);
                }

                ApiClient.InitializeApiClient();
                
                foreach (string item in items.City)
                {
                    log.WriteLine($"Checking weather conditions for {item}", LogType.INFO);

                    WeatherModel itemWM = WeatherProcessor.GetWeatherDetails(item, Units.Metric, log);
                    float tempAPI = 0;
                    if (itemWM != null)
                    {
                        tempAPI = itemWM.main.Temp;

                        SeleniumHelper seleniumHelper = new SeleniumHelper(log);
                        float tempUI;

                        if (!seleniumHelper.GetCurrentCityTemperature(item, out tempUI))
                        {
                            log.WriteLine("Failed to Initialize Selenium Web Driver", LogType.ERROR);
                        }
                        if (Math.Abs(tempAPI - tempUI) > items.Variance)
                        {
                            throw new MatcherException($"Temperature variance has been found to be more than {items.Variance} for {item}");
                        }
                    }
                }
            }
            finally
            {
                ApiClient.Dispose();

                if(log != null)
                    log.Dispose();
            }
        }
    }

    public class JsonInputData
    {
        public string[] City { get; set; }
        public int Variance { get; set; }
    }

    public class MatcherException : Exception
    {
        public MatcherException(string message) : base($"Matcher Exception: {message}")
        {

        }
    }

}
