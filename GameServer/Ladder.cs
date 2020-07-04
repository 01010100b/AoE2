using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameServer
{
    public class Ladder
    {
        public static int GetEloDelta(int elo1, int elo2, double result, int k1, int k2)
        {
            var k = (k1 + k2) / 2;
            var ev = 1 / (1 + Math.Pow(10, (elo2 - elo1) / 400.0));
            var delta = k * (result - ev);

            if (delta >= 0 && delta < 1)
            {
                delta = 1;
            }
            else if (delta < 0 && delta > -1)
            {
                delta = -1;
            }

            return (int)Math.Round(delta);
        }

        private class Settings
        {
            public string Exe { get; set; } = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "Age2_x1", "age2_x1.5.exe");
            public int Speed { get; set; } = 100;
            public string AiFolder { get; set; } = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "Ai");
            public string RecFolder { get; set; } = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "SaveGame");
        }

        private class AI
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Elo { get; set; }
            public int Games { get; set; }
            public long LatestUpdate { get; set; }
        }

        private class SavedGame
        {
            public int Id { get; set; }
            public Game Game { get; set; }
            public bool Crashed { get; set; }
            public string Winner { get; set; }
        }

        private const string TEMP_FOLDER = @"E:\ladder";
        private readonly List<AI> AIs = new List<AI>();
        private HttpClient Client;
        private volatile bool Stop = false;

        public void Run()
        {
            var settings = new Settings();

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json")))
            {
                Debug.WriteLine("loading settings");
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json")));
            }

            Client = new HttpClient();

            var runner = new Runner(settings.Exe, settings.Speed, settings.AiFolder, settings.RecFolder);
            
            while (!Stop)
            {
                runner.Startup();
                Debug.WriteLine("startup done");

                Debug.WriteLine("getting next game");
                var game = GetNextGame();

                Console.WriteLine(PrintRanking());
                Console.WriteLine("");
                Trace.WriteLine(PrintRanking());
                Trace.WriteLine("");

                Debug.WriteLine("running game " + DateTime.Now);
                var result = runner.Run(game);
                Debug.WriteLine("adding result");
                SetResult(result);

                Debug.WriteLine("shutting down runner");
                runner.Shutdown();
                try
                {
                    
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            Client.Dispose();
        }

        private Game GetNextGame()
        {
            Load();

            if (AIs.Count < 2)
            {
                throw new Exception("Need at least 2 AIs");
            }

            var rng = new Random();
            var name1 = AIs[rng.Next(AIs.Count)].Name;
            if (rng.NextDouble() < 2d / AIs.Count)
            {
                AIs.Sort((a, b) => a.Games.CompareTo(b.Games));
                name1 = AIs[0].Name;
            }
            if (rng.NextDouble() < 0.3)
            {
                name1 = "Binary";
            }

            var name2 = AIs[rng.Next(AIs.Count)].Name;
            while (name2 == name1)
            {
                name2 = AIs[rng.Next(AIs.Count)].Name;
            }

            var folder1 = GetLatestVersionFolder(name1);
            var folder2 = GetLatestVersionFolder(name2);

            var game = new Game();
            var teamsize = rng.Next(1, 5);
            for (int i = 1; i <= teamsize; i++)
            {
                var player1 = new Player() { Name = name1, Team = 1, Civ = 19, Folder = folder1 };
                var player2 = new Player() { Name = name2, Team = 2, Civ = 19, Folder = folder2 };

                game.Players.Add(player1);
                game.Players.Add(player2);
            }

            switch (teamsize)
            {
                case 1: game.MapSize = 0; break;
                case 2: game.MapSize = 2; break;
                case 3: game.MapSize = 3; break;
                case 4: game.MapSize = 4; break;
            }

            return game;
        }

        private void SetResult(GameResult result)
        {

        }

        private void Load()
        {
            AIs.Clear();

            var src = Client.GetAsync(@"https://aiscripters.projectvalkyrie.net/ladder/auto_ais.php").Result.Content.ReadAsStringAsync().Result;
            //Debug.WriteLine(src);
            var lines = src.Split("\n").Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
            foreach (var line in lines)
            {
                var pieces = line.Split(",");
                var ai = new AI() 
                { 
                    Id = int.Parse(pieces[0]), 
                    Name = pieces[1], 
                    Elo = int.Parse(pieces[2]), 
                    Games = int.Parse(pieces[3]), 
                    // TODO add civs pieces[4]
                    LatestUpdate = long.Parse(pieces[5]) 
                };
                AIs.Add(ai);
            }

            foreach (var ai in AIs)
            {
                var folder = Path.Combine(TEMP_FOLDER, "ais", ai.Name, ai.LatestUpdate.ToString());
                if (!Directory.Exists(folder))
                {
                    var url = @"https://aiscripters.projectvalkyrie.net/ladder/files/ais/" + ai.Name + ".zip";
                    Debug.WriteLine(url);
                    var bytes = Client.GetAsync(url).Result.Content.ReadAsByteArrayAsync().Result;
                    var file = Path.Combine(TEMP_FOLDER, "temp.dat");
                    File.WriteAllBytes(file, bytes);
                    ZipFile.ExtractToDirectory(file, folder);
                }
            }
        }

        private string GetLatestVersionFolder(string name)
        {
            var folders = Directory.EnumerateDirectories(Path.Combine(TEMP_FOLDER, "ais", name)).Select(d => Path.GetFileName(d)).ToList();
            if (folders.Count < 1)
            {
                throw new Exception("AI " + name + " does not have a latest version folder");
            }
            folders.Sort((a, b) => long.Parse(b).CompareTo(long.Parse(a)));

            return Path.Combine(TEMP_FOLDER, "ais", name, folders[0]);
        }

        private string PrintRanking()
        {
            AIs.Sort((a, b) => b.Elo.CompareTo(a.Elo));
            var sb = new StringBuilder();
            sb.AppendLine("--- RANKING ---");
            sb.AppendLine("Rank Name                     Elo  Games");
            
            for (int i = 0; i < AIs.Count; i++)
            {
                var ai = AIs[i];
                var str = string.Format("{0,-4} {1, -24} {2, -4} {3, -4}", i + 1, ai.Name, ai.Elo, ai.Games);
                sb.AppendLine(str);
            }

            return sb.ToString();
        }
    }
}
