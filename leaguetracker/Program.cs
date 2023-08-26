using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using Newtonsoft.Json;

namespace leaguetracker
{
    class Program
    {
        static bool exitRequested = false;

        static void Main()
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // This prevents the program from terminating immediately
                exitRequested = true;
            };

            string configFilePath = "config.json";
            var config = ReadConfig(configFilePath);

            Console.WriteLine("Welcome to the League of Legends Playtime Tracker.");

            double totalSeconds = GetTotalPlaytimeFromCsv(config);
            Console.WriteLine($"Total playtime: {FormatSeconds(totalSeconds)}\nLast Time Played: {GetLastTimePlayedFromCsv(config)}");

            while (true)
            {
                Console.WriteLine("\nMenu:");
                Console.WriteLine("1. Start League of Legends");
                Console.WriteLine("2. Open tracker.csv");
                Console.WriteLine("3. Exit.");
                Console.Write("Enter your choice: ");

                int choice;
                if (int.TryParse(Console.ReadLine(), out choice))
                {
                    switch (choice)
                    {
                        case 1:
                            StartLeagueOfLegends(config);
                            break;
                        case 2:
                            try
                            {
                                Process.Start("code", config.TrackerFilePath);
                            }
                            catch (Exception ex)
                            {
                                Process.Start("notepad", config.TrackerFilePath);
                            }
                            break;
                        case 3:
                            return; // Exit the program
                        default:
                            Console.WriteLine("Invalid choice. Please select a valid option.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }

                if (exitRequested)
                {
                    return; // Exit the program
                }
            }
        }

        static string GetLastTimePlayedFromCsv(AppConfig config)
        {
            const string defaultHasNotBeenPlayed = "This game has not been played yet.";

            if (!File.Exists(config.TrackerFilePath))
            {
                File.WriteAllText(config.TrackerFilePath, "App Name,Time Played,DateTime Started,DateTime Ended\n");
                return defaultHasNotBeenPlayed;
            }

            var lastLine = File.ReadLines(config.TrackerFilePath).LastOrDefault();
            if (string.IsNullOrEmpty(lastLine) || lastLine.StartsWith("App Name,"))
            {
                return defaultHasNotBeenPlayed;
            }

            var columns = lastLine.Split(',');
            // Assuming the DateTime Started is in the third column (index 2)
            if (DateTime.TryParse(columns[2], out DateTime lastPlayed))
            {
                return lastPlayed.ToString();
            }

            return defaultHasNotBeenPlayed;
        }


        static double GetTotalPlaytimeFromCsv(AppConfig config)
        {
            if (!File.Exists(config.TrackerFilePath))
            {
                File.WriteAllText(config.TrackerFilePath, "App Name,Time Played,DateTime Started,DateTime Ended\n");
                return 0;
            }

            var lines = File.ReadAllLines(config.TrackerFilePath).Skip(1); // Skip header
            return lines.Sum(line => double.Parse(line.Split(',')[1]));
        }

        static void AppendToCsv(AppConfig config, double timePlayed, DateTime start, DateTime end)
        {
            var newRow = $"{config.AppName},{timePlayed},{start},{end}\n";
            File.AppendAllText(config.TrackerFilePath, newRow);
        }

        static void StartLeagueOfLegends(AppConfig config)
        {
            DateTime startTime = DateTime.Now;
            Process process = Process.Start(config.GameClientPath);

            while (!HasProcessExited(process.Id))
            {
                System.Threading.Thread.Sleep(1000); // Check every second
            }

            DateTime endTime = DateTime.Now;
            double elapsedSeconds = (endTime - startTime).TotalSeconds;

            AppendToCsv(config, elapsedSeconds, startTime, endTime);
            Console.WriteLine($"You played for: {FormatSeconds(elapsedSeconds)}");
        }

        public static string FormatSeconds(double totalSeconds)
        {
            int hours = (int)(totalSeconds / 3600);
            int remainder = (int)(totalSeconds % 3600);
            int minutes = remainder / 60;
            int seconds = remainder % 60;

            return $"{hours} Hours {minutes} Minutes {seconds} Seconds";
        }

        static bool HasProcessExited(int processId)
        {
            // Check if the main process has exited
            try
            {
                Process.GetProcessById(processId);
            }
            catch (ArgumentException)
            {
                // Main process has exited, now check child processes
                var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_Process WHERE ParentProcessId={processId}");
                foreach (var _ in searcher.Get())
                {
                    // If a child process is found, the main process hasn't fully exited
                    return false;
                }

                // If we're here, both the main process and its children have exited
                return true;
            }

            // If we're here, the main process is still running
            return false;
        }

        public static AppConfig ReadConfig(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new AppConfig
                {
                    GameClientPath = "",
                    AppName = "",
                    TrackerFilePath = ""
                };

                Console.WriteLine("No config.json found, starting first-time configuration!\n");

                Console.WriteLine("Please provide the path to the League of Legends game client:");
                defaultConfig.GameClientPath = Console.ReadLine().Replace("\"", string.Empty); // Ensure the user didn't accidentally use additional quotes.


                Console.WriteLine("Please provide the app name of the game client:");
                defaultConfig.AppName = Console.ReadLine().Replace("\"", string.Empty);

                Console.WriteLine("Please provide the file path where the tracker.csv should be stored:");
                defaultConfig.TrackerFilePath = Console.ReadLine().Replace("\"", string.Empty);

                File.WriteAllText(configFilePath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
                return defaultConfig;
            }

            var configJson = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<AppConfig>(configJson);
        }

    }
}
