using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
            public string Name { get; set; }
            public string Author { get; set; }
            public int Elo { get; set; }
            public int Games { get; set; }
        }

        private class SavedGame
        {
            public int Id { get; set; }
            public Game Game { get; set; }
            public bool Crashed { get; set; }
            public string Winner { get; set; }
        }

        private const string LADDER_FOLDER = @"E:\ladder";
        private readonly List<AI> AIs = new List<AI>();
        private readonly List<SavedGame> SavedGames = new List<SavedGame>();
        private volatile bool Stop = false;

        public void Run()
        {
            var settings = new Settings();

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json")))
            {
                Debug.WriteLine("loading settings");
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json")));
            }

            var runner = new Runner(settings.Exe, settings.Speed, settings.AiFolder, settings.RecFolder);
            runner.Startup();
            Debug.WriteLine("startup done");

            var reset = false;
            while (!Stop)
            {
                try
                {
                    Debug.WriteLine("getting next game");

                    var game = GetNextGame();
                    Console.WriteLine(PrintRanking());
                    Console.WriteLine();
                    Trace.WriteLine(PrintRanking());
                    Trace.WriteLine("");

                    Debug.WriteLine("running game");
                    var result = runner.Run(game);
                    Debug.WriteLine("adding result");
                    SetResult(result);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    reset = true;
                }

                if (reset)
                {
                    try
                    {
                        Debug.WriteLine("Restarting runner");
                        runner.Shutdown();
                        runner.Startup();
                        reset = false;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }

            runner.Shutdown();
        }

        protected virtual Game GetNextGame()
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

            return game;
        }

        protected virtual void SetResult(GameResult result)
        {
            var savedgame = new SavedGame();
            var id = 1;
            if (SavedGames.Count > 0)
            {
                id = SavedGames.Max(g => g.Id);
                id++;
            }
            savedgame.Id = id;
            savedgame.Game = result.Game;
            savedgame.Crashed = result.Crashed;
            if (!savedgame.Crashed)
            {
                savedgame.Winner = result.Winners[0].Name;

                var players = result.Game.Players.Select(p => p.Name).Distinct().Select(n => AIs.Single(a => a.Name == n)).ToList();
                Debug.Assert(players.Count == 2);

                var score = 0;
                if (savedgame.Winner == players[0].Name)
                {
                    score = 1;
                }

                var elo1 = players[0].Elo;
                var k1 = Math.Max(10, 30 - players[0].Games);
                var elo2 = players[1].Elo;
                var k2 = Math.Max(10, 30 - players[1].Games);
                var delta = GetEloDelta(elo1, elo2, score, k1, k2);

                players[0].Elo += delta;
                players[1].Elo -= delta;
                players[0].Games++;
                players[1].Games++;
            }

            SavedGames.Add(savedgame);

            if (result.Rec != null)
            {
                var file = Path.Combine(LADDER_FOLDER, "recs", id + ".mgz");
                File.WriteAllBytes(file, result.Rec);
            }

            Save();
        }

        private void Load()
        {
            AIs.Clear();
            SavedGames.Clear();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var file = Path.Combine(LADDER_FOLDER, "ais.json");
            if (File.Exists(file))
            {
                var ais = JsonSerializer.Deserialize<List<AI>>(File.ReadAllText(file), options);
                AIs.AddRange(ais);
            }

            file = Path.Combine(LADDER_FOLDER, "games.json");
            if (File.Exists(file))
            {
                var games = JsonSerializer.Deserialize<List<SavedGame>>(File.ReadAllText(file), options);
                SavedGames.AddRange(games);
            }

            if (AIs.Count < 2)
            {
                AIs.Clear();
                var binary = new AI() { Name = "Binary", Author = "01010100", Elo = 1000, Games = 0 };
                var barbarian = new AI() { Name = "Barbarian", Author = "TheMax", Elo = 1000, Games = 0 };
                AIs.Add(binary);
                AIs.Add(barbarian);
            }
        }

        private void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var file = Path.Combine(LADDER_FOLDER, "ais.json");
            File.WriteAllText(file, JsonSerializer.Serialize(AIs, options));

            file = Path.Combine(LADDER_FOLDER, "games.json");
            File.WriteAllText(file, JsonSerializer.Serialize(SavedGames, options));
        }

        private string GetLatestVersionFolder(string name)
        {
            var folders = Directory.EnumerateDirectories(Path.Combine(LADDER_FOLDER, "bots", name)).Select(d => Path.GetFileName(d)).ToList();
            if (folders.Count < 1)
            {
                throw new Exception("AI " + name + " does not have a latest version folder");
            }
            folders.Sort((a, b) => int.Parse(b).CompareTo(int.Parse(a)));

            return Path.Combine(LADDER_FOLDER, "bots", name, folders[0]);
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
