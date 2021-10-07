using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace BlueStacksAssignment
{
    class SeleniumHelper
    {
        public string Url { get; set; }
        private Log _log = null;
        private IWebDriver webDriver = null;

        public SeleniumHelper(Log log)
        {
            _log = log;
        }

        public bool GetCurrentCityTemperature(string cityName, out float cityTemp)
        {
            string cityTempInfo = string.Empty;
            bool result = false;
            cityTemp = 0;

            if (!File.Exists("chromedriver.exe"))
            {
                _log.WriteLine("chromedriver doesn't exist in local directory", LogType.ERROR);
                throw new Exception("chromedriver was not found");
            }

            try
            {
                string chromeAppPath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\ChromeHTML\Application", "ApplicationIcon", "");
                if (chromeAppPath == null)
                {
                    if(ExecuteProcess("ChromeSetup.exe", "", 120 * 1000, out cityTempInfo) != 0)
                    {
                        _log.WriteLine("Failed during installing Chrome Browser", LogType.ERROR);
                        return result;
                    }

                    chromeAppPath = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\ChromeHTML\Application", "ApplicationIcon", "");
                    if (chromeAppPath == null)
                    {
                        _log.WriteLine("Failed to Install chrome browser", LogType.ERROR);
                        return result;
                    }
                }

                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("start-maximized");
                chromeOptions.BinaryLocation = chromeAppPath.Replace(",0", "");
                ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();

                webDriver = new ChromeDriver(chromeDriverService, chromeOptions);
                if (webDriver == null)
                {
                    _log.WriteLine("Failed to initialize Chrome Webdriver", LogType.ERROR);
                    return result;
                }

                webDriver.Navigate().GoToUrl("https://weather.com/en-IN/");
                Thread.Sleep(3000);

                IWebElement searchBox = webDriver.FindElement(By.Id("LocationSearch_input"));

                if(searchBox == null)
                {
                    _log.WriteLine("Failed to find Search textbox", LogType.ERROR);
                    return result;
                }
                searchBox.Click();
                searchBox.SendKeys(cityName);

                Thread.Sleep(2000);

                IWebElement searchResultsListBox_Button0 = webDriver.FindElement(By.Id("LocationSearch_listbox-0"));

                if(searchResultsListBox_Button0 == null)
                {
                    _log.WriteLine("Failed to find Search ResultsListBox_Button0", LogType.ERROR);
                    return result;
                }

                searchResultsListBox_Button0.Click();

                IWebElement temp = webDriver.FindElement(By.XPath("//span[@data-testid='TemperatureValue']"));

                if(temp == null)
                {
                    _log.WriteLine("Failed to find temperature", LogType.ERROR);
                    return result;
                }
                if (!float.TryParse(temp.Text.Remove(temp.Text.Length - 1), out cityTemp))
                {
                    _log.WriteLine($"Failed to fetch Current temperature of {cityName}", LogType.ERROR);
                    return result;
                }

                result = true;
            }
            catch (InvalidOperationException iOpEx)
            {
                _log.WriteLine("Invalid Operation Exception has been caught during initializing Selenium Web driver", LogType.ERROR);
                _log.WriteLine($"Message: {iOpEx.Message}", LogType.ERROR);
            }
            catch (Exception ex)
            {
                _log.WriteLine("Exception caught in fetching Current City weather details");
            }
            finally
            {
                if (webDriver != null)
                    webDriver.Close();
                KillProcess("chrome");
            }

            return result;
        }

        public static int ExecuteProcess(string filename, string arguments, int timeout_in_miliseconds, out string cityTemp, string workingDirectory = "")
        {
            var exitcode = 0;
            cityTemp = string.Empty;
            Log ProcessLog = new Log("Processx.log", true);
            ProcessLog.WriteLine($"================================================================================");
            ProcessLog.WriteLine($"Process exe : {filename} , arguments : {arguments}");
            ProcessLog.WriteLine($"================================================================================");
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = filename;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                if (!string.IsNullOrEmpty(workingDirectory))
                    process.StartInfo.WorkingDirectory = workingDirectory;
                //Start the process
                process.Start();
                //wait for the process to Exit in timeout_in_miliseconds 
                process.WaitForExit(timeout_in_miliseconds);
                var psOut = process.StandardOutput;
                var psErr = process.StandardError;

                cityTemp = string.Empty;
                //check if the process has exited
                if (process.HasExited)
                {
                    exitcode = process.ExitCode;
                    if (exitcode == 0)
                    {
                        ProcessLog.WriteLine("Output of process is as following: ");
                        cityTemp = psOut.ReadToEnd();
                    }
                    else
                    {
                        ProcessLog.WriteLine("Error while running the process.Details: ");
                        cityTemp = psErr.ReadToEnd();      //Incase of error                        
                    }
                }
                else
                {
                    if (!process.StartInfo.FileName.ToLower().Contains("cmd"))
                    {
                        ProcessLog.WriteLine("Output of process is as following: ");
                        cityTemp = psOut.ReadToEnd();
                    }
                }
                ProcessLog.WriteLine(cityTemp);
                ProcessLog.WriteLine("");
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                ProcessLog.WriteLine("Exception caught while running process: " + ex.ToString());
                exitcode = -1;
            }
            return exitcode;
        }

        public static bool KillProcess(string processName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (Process process in processes)
                {
                    if (Process.GetCurrentProcess().Id != process.Id)
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(2000);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught while killing process" + ex.Message);
            }

            return false;
        }
    }
}
