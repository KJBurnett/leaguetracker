using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace leaguetracker
{
    class Program
    {
        private const string TrackerFilePath = "C:\\Users\\burne\\OneDrive\\Documents\\leaguetracker\\tracker.csv";
        private const string vscodeFilePath = "C:\\Users\\burne\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe";
        private const string appPath = "E:\\Games\\Riot Games\\League of Legends\\LeagueClient.exe";
        private const string AppName = "League of Legends";
        static bool exitRequested = false;

        static void Main()
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // This prevents the program from terminating immediately
                exitRequested = true;
            };

            Console.WriteLine("Welcome to the League of Legends Playtime Tracker.");

            double totalSeconds = GetTotalPlaytimeFromCsv();
            Console.WriteLine($"Total playtime: {FormatSeconds(totalSeconds)}\nLast Time Played: {GetLastTimePlayedFromCsv()}");

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
                            StartLeagueOfLegends();
                            break;
                        case 2:
                            Process.Start(vscodeFilePath, TrackerFilePath);
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

        static string GetLastTimePlayedFromCsv()
        {
            const string defaultHasNotBeenPlayed = "This game has not been played yet.";

            if (!File.Exists(TrackerFilePath))
            {
                File.WriteAllText(TrackerFilePath, "App Name,Time Played,DateTime Started,DateTime Ended\n");
                return defaultHasNotBeenPlayed;
            }

            var lastLine = File.ReadLines(TrackerFilePath).LastOrDefault();
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


        static double GetTotalPlaytimeFromCsv()
        {
            if (!File.Exists(TrackerFilePath))
            {
                File.WriteAllText(TrackerFilePath, "App Name,Time Played,DateTime Started,DateTime Ended\n");
                return 0;
            }

            var lines = File.ReadAllLines(TrackerFilePath).Skip(1); // Skip header
            return lines.Sum(line => double.Parse(line.Split(',')[1]));
        }

        static void AppendToCsv(string appName, double timePlayed, DateTime start, DateTime end)
        {
            var newRow = $"{appName},{timePlayed},{start},{end}\n";
            File.AppendAllText(TrackerFilePath, newRow);
        }

        static void StartLeagueOfLegends()
        {
            DateTime startTime = DateTime.Now;
            Process process = Process.Start(appPath);

            while (!HasProcessExited(process.Id))
            {
                System.Threading.Thread.Sleep(1000); // Check every second
            }

            DateTime endTime = DateTime.Now;
            double elapsedSeconds = (endTime - startTime).TotalSeconds;

            AppendToCsv(AppName, elapsedSeconds, startTime, endTime);
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
    }
}
