using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new Settings();

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json")))
            {
                Debug.WriteLine("loading settings");
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json")));
            }

            Console.Write("Password (leave blank for local test): ");
            var pw = Console.ReadLine();

            var ladder = new Ladder();
            ladder.Run(settings, pw);
        }
    }
}
