﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private class AI
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Elo { get; set; }
            public int Games { get; set; }
            public long LatestUpdate { get; set; }
            public Civ Civ { get; set; }
        }

        private class SavedGame
        {
            public int Id { get; set; }
            public Game Game { get; set; }
            public bool Crashed { get; set; }
            public string Winner { get; set; }
        }

        private string TEMP_FOLDER => AppDomain.CurrentDomain.BaseDirectory;
        private readonly List<AI> AIs = new List<AI>();
        private HttpClient Client;
        private volatile bool Stop = false;
        private Settings Settings;
        private string Password;

        public void Run(Settings settings, string pw = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            Settings = settings;
            Password = pw;

            Client = new HttpClient();

            var runner = new Runner(Settings.Exe, Settings.Speed, Settings.AiFolder, Settings.RecFolder);
            
            while (!Stop)
            {
                try
                {
                    Load();

                    Console.WriteLine(PrintRanking());
                    Console.WriteLine("");

                    Console.WriteLine("getting next game");
                    var game = GetNextGame();

                    Console.WriteLine("running game ");
                    var result = runner.Run(game);
                    Console.WriteLine("uploading result");
                    SetResult(result);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message + "\n" + e.StackTrace);
                }
            }

            Client.Dispose();
        }

        private Game GetNextGame()
        {
            if (AIs.Count < 2)
            {
                throw new Exception("Need at least 2 AIs");
            }

            var rng = new Random();
            var ai1 = AIs[rng.Next(AIs.Count)];
            if (rng.NextDouble() < 2d / AIs.Count)
            {
                AIs.Sort((a, b) => a.Games.CompareTo(b.Games));
                ai1 = AIs[0];
            }

            var ai2 = AIs[rng.Next(AIs.Count)];
            while (ai2 == ai1)
            {
                ai2 = AIs[rng.Next(AIs.Count)];
            }

            var folder1 = GetLatestVersionFolder(ai1.Name);
            var folder2 = GetLatestVersionFolder(ai2.Name);

            var game = new Game();
            var teamsize = rng.Next(1, 5);
            for (int i = 1; i <= teamsize; i++)
            {
                var player1 = new Player() { Name = ai1.Name, Team = 1, Civ = ai1.Civ, Folder = folder1 };
                var player2 = new Player() { Name = ai2.Name, Team = 2, Civ = ai2.Civ, Folder = folder2 };

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
            if (string.IsNullOrWhiteSpace(result.RecFile))
            {
                return;
            }

            if (result.Crashed)
            {
                return;
            }

            if (!File.Exists(result.RecFile))
            {
                throw new FileNotFoundException(result.RecFile);
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                Console.WriteLine("No password, aborting upload");
                File.Delete(result.RecFile);
                return;
            }

            var dir = Path.Combine(TEMP_FOLDER, "tmp");
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            Directory.CreateDirectory(dir);
            File.Move(result.RecFile, Path.Combine(dir, Path.GetFileName(result.RecFile)));

            var file = Path.Combine(TEMP_FOLDER, "temp.zip");
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            ZipFile.CreateFromDirectory(dir, file);

            var rng = new Random();

            var winner = AIs.Single(a => a.Name == result.Winners[0].Name);
            var loser = AIs.Single(a => a.Name == result.Game.Players.First(p => p.Name != winner.Name).Name);

            var pw = Password;

            var player1 = winner.Id.ToString();
            var player2 = loser.Id.ToString();
            var player1_wins = "1";
            var player2_wins = "0";
            if (rng.NextDouble() < 0.5)
            {
                player1 = loser.Id.ToString();
                player2 = winner.Id.ToString();
                player1_wins = "0";
                player2_wins = "1";
            }

            var teams = "1v1";
            switch (result.Game.Players.Count)
            {
                case 4: teams = "2v2"; break;
                case 6: teams = "3v3"; break;
                case 8: teams = "4v4"; break;
            }

            var map = result.Game.MapType.ToString();
            switch (result.Game.MapType)
            {
                case Map.BlackForest: map = "Black Forest"; break;
                case Map.CraterLake: map = "Crater Lake"; break;
                case Map.GhostLake: map = "Ghost Lake"; break;
                case Map.GoldRush: map = "Gold Rush"; break;
                case Map.SaltMarsh: map = "Salt March"; break;
                case Map.TeamIslands: map = "Team Islands"; break;
            }

            var game_mode = "Random Map";
            var tournament = "Auto Ladder";
            var game_version = "WK";

            using var form = new MultipartFormDataContent();
            using var file_content = new ByteArrayContent(File.ReadAllBytes(file));
            file_content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(file_content, "zip", Path.GetFileName(file));

            form.Add(new StringContent(pw), "pw");
            form.Add(new StringContent(player1), "player1");
            form.Add(new StringContent(player2), "player2");
            form.Add(new StringContent(player1_wins), "player1_wins");
            form.Add(new StringContent(player2_wins), "player2_wins");
            form.Add(new StringContent(teams), "teams");
            form.Add(new StringContent(map), "map");
            form.Add(new StringContent(game_mode), "game_mode");
            form.Add(new StringContent(tournament), "tournament");
            form.Add(new StringContent(game_version), "game_version");

            var url = @"https://aiscripters.projectvalkyrie.net/ladder/add_game.php";
            var response = Client.PostAsync(url, form).Result;
            var content = response.Content;
            Debug.WriteLine(content.ReadAsStringAsync().Result);
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
                    Civ = Enum.Parse<Civ>(pieces[4]),
                    LatestUpdate = long.Parse(pieces[5]) 
                };
                AIs.Add(ai);
            }

            foreach (var ai in AIs.ToList())
            {
                var folder = Path.Combine(TEMP_FOLDER, "ais", ai.Name, ai.LatestUpdate.ToString());
                if (!Directory.Exists(folder))
                {
                    try
                    {
                        var url = @"https://aiscripters.projectvalkyrie.net/ladder/files/ais/" + ai.Name + ".zip";
                        Console.WriteLine("Downloading: " + url);
                        var bytes = Client.GetAsync(url).Result.Content.ReadAsByteArrayAsync().Result;
                        var file = Path.Combine(TEMP_FOLDER, "temp.zip");
                        File.WriteAllBytes(file, bytes);
                        ZipFile.ExtractToDirectory(file, folder, true);

                        var ai_file = Directory.EnumerateFiles(folder, "*.ai").Single();
                        var old_name = Path.GetFileNameWithoutExtension(ai_file);
                        var per_file = Directory.EnumerateFiles(folder, "*.per")
                            .Where(f => Path.GetFileNameWithoutExtension(f) == old_name).Single();

                        File.Move(ai_file, Path.Combine(folder, ai.Name + ".ai"));
                        File.Move(per_file, Path.Combine(folder, ai.Name + ".per"));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message + "\n" + e.StackTrace);
                        AIs.Remove(ai);
                    }
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
