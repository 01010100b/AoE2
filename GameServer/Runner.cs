﻿using Bleak;
using Microsoft.Win32;
using MsgPack;
using MsgPack.Rpc.Core.Client;
using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace GameServer
{
    class Runner
    {
        public readonly string Exe;
        public readonly int Speed;
        public readonly string AiFolder;
        public readonly string RecFolder;

        private Process Process = null;
        private RpcClient RpcClient = null;

        public Runner(string exe, int speed, string ai_folder, string rec_folder)
        {
            Exe = exe;
            Speed = speed;
            AiFolder = ai_folder;
            RecFolder = rec_folder;

            if (!File.Exists(Exe))
            {
                throw new Exception("File not found: " + Exe);
            }

            if (!Directory.Exists(AiFolder))
            {
                throw new Exception("Folder not found: " + AiFolder);
            }

            if (!Directory.Exists(RecFolder))
            {
                throw new Exception("Folder not found: " + RecFolder);
            }
        }

        public GameResult Run(Game game)
        {
            try
            {
                if (RpcClient != null)
                {
                    RpcClient.Dispose();
                }

                if (Process != null && !Process.HasExited)
                {
                    Process.Kill();
                    Process.WaitForExit();
                    Thread.Sleep(5000);
                }

                var rng = new Random();
                var port = rng.Next(40000, 60000);

                Process = Launch(Exe, port);
                RpcClient = new RpcClient(new IPEndPoint(IPAddress.Loopback, port));
                Thread.Sleep(1000);

                var api = RpcClient.Call("GetApiVersion");
                Debug.WriteLine("Api: " + api.ToString());

                return RunPrivate(game);
            }
            finally
            {
                RpcClient?.Dispose();

                if (Process != null && !Process.HasExited)
                {
                    Process.Kill();
                    Process.WaitForExit();
                    Thread.Sleep(5000);
                } 
            }
        }

        private GameResult RunPrivate(Game game)
        {
            if (Process == null || Process.HasExited)
            {
                throw new Exception("Process not running");
            }

            // install players

            foreach (var player in game.Players)
            {
                InstallPlayer(player);
            }

            // start game

            Call("ResetGameSettings");

            Call("SetGameType", game.GameType);
            if (game.GameType == 3)
            {
                Call("SetGameScenarioName", game.ScenarioName);
            }
            
            Call("SetGameMapType", (int)game.MapType);
            Call("SetGameMapSize", game.MapSize);
            Call("SetGameDifficulty", game.Difficulty);
            Call("SetGameStartingResources", game.StartingResources);
            Call("SetGamePopulationLimit", game.PopulationLimit);
            Call("SetGameRevealMap", game.RevealMap);
            Call("SetGameStartingAge", game.StartingAge);
            Call("SetGameVictoryType", game.VictoryType, game.VictoryValue);
            Call("SetGameTeamsTogether", game.TeamsTogether);
            Call("SetGameTeamsLocked", game.LockTeams);
            Call("SetGameAllTechs", game.AllTechs);
            Call("SetGameRecorded", game.Recorded);

            for (int i = 0; i < 8; i++)
            {
                var player = i + 1;
                if (i < game.Players.Count)
                {
                    Call("SetPlayerComputer", player, game.Players[i].Name);
                    Call("SetPlayerCivilization", player, (int)game.Players[i].Civ);
                    Call("SetPlayerTeam", player, game.Players[i].Team);
                }
                else
                {
                    Call("SetPlayerClosed", player);
                }
            }

            Call("SetUseInGameResolution", false);
            Call("SetRunUnfocused", true);
            Call("SetWindowMinimized", true);

            Call("StartGame");

            // wait for finish
            var finished = false;
            var crashed = false;
            var start = DateTime.UtcNow;
            while (!finished)
            {
                Thread.Sleep(2000);

                if (Process.HasExited)
                {
                    crashed = true;
                    break;
                }

                if (DateTime.UtcNow - start > TimeSpan.FromHours(2))
                {
                    crashed = true;
                    break;
                }

                if (Call("GetGameTime").AsInt32() > 2 * 60 * 60)
                {
                    break;
                }

                var team0 = 0;
                var team1 = 0;
                var team2 = 0;
                var team3 = 0;
                var team4 = 0;
                var players_in_game = 0;
                var team_max = 0;

                for (int i = 0; i < 8; i++)
                {
                    if (Call("GetPlayerAlive", i + 1).AsBoolean())
                    {
                        Thread.Sleep(300);

                        players_in_game++;

                        if (game.Players[i].Team == 0)
                        {
                            team0++;
                        }

                        if (game.Players[i].Team == 1)
                        {
                            team1++;
                            team_max = Math.Max(team1, team_max);
                        }

                        if (game.Players[i].Team == 2)
                        {
                            team2++;
                            team_max = Math.Max(team2, team_max);
                        }

                        if (game.Players[i].Team == 3)
                        {
                            team3++;
                            team_max = Math.Max(team3, team_max);
                        }

                        if (game.Players[i].Team == 4)
                        {
                            team4++;
                            team_max = Math.Max(team4, team_max);
                        }
                    }
                }

                finished = true;

                if (team0 > 1)
                {
                    finished = false;
                }

                if (players_in_game > team_max && players_in_game > team0)
                {
                    finished = false;
                }
            }

            // get result
            var result = new GameResult(game, crashed);
            if (result.Crashed)
            {
                Debug.WriteLine("game crashed");
            }
            else
            {
                if (finished)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (Call("GetPlayerAlive", i + 1).AsBoolean())
                        {
                            Thread.Sleep(300);
                            result.Winners.Add(game.Players[i]);
                        }
                    }
                }
                else
                {
                    var max = -1;
                    var winner = -1;
                    for (int i = 0; i < 8; i++)
                    {
                        var score = Call("GetPlayerScore", i + 1).AsInt32();
                        Thread.Sleep(300);
                        if (score > max)
                        {
                            max = score;
                            winner = i;
                        }
                    }

                    if (winner >= 0)
                    {
                        result.Winners.Add(game.Players[winner]);
                    }
                }

                foreach (var player in game.Players.Where(p => p.Team != 0 && !result.Winners.Contains(p)))
                {
                    var team = player.Team;
                    foreach (var p in result.Winners)
                    {
                        if (p.Team == team)
                        {
                            result.Winners.Add(player);
                            break;
                        }
                    }
                }

                Call("QuitGame");
                Thread.Sleep(5000);

                if (game.Recorded)
                {
                    var latest = DateTime.MinValue;
                    var rec = "";
                    foreach (var file in Directory.EnumerateFiles(RecFolder, "*.mgz"))
                    {
                        var fi = new FileInfo(file);
                        if (fi.LastWriteTimeUtc > latest)
                        {
                            latest = fi.LastWriteTimeUtc;
                            rec = file;
                        }
                    }

                    if (File.Exists(rec))
                    {
                        result.RecFile = rec;
                    }
                }
                
            }

            return result;
        }

        private Process Launch(string exe, int port)
        {
            Debug.WriteLine("launching from: " + exe);

            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0");
            var speed = (int)key.GetValue("Game Speed");

            var dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aoc-auto-game.dll");
            if (!File.Exists(dll))
            {
                throw new Exception("File not found: " + dll);
            }

            if (!File.Exists(exe))
            {
                throw new Exception("File not found: " + exe);
            }

            Process process = null;
            try
            {
                key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0", true);
                key.SetValue("Game Speed", Speed);

                process = Process.Start(exe, "-multipleinstances -autogameport " + port);

                while (!process.Responding)
                {
                    Debug.WriteLine("Waiting for process to respond");
                    Thread.Sleep(1 * 1000);
                }

                Thread.Sleep(3 * 1000);
                using (var injector = new Injector(process.Id, dll, InjectionMethod.CreateThread, InjectionFlags.None))
                {
                    injector.InjectDll();
                }

                Thread.Sleep(10 * 1000);
            }
            catch (Exception e)
            {
                if (process != null && !process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }

                throw e;
            }
            finally
            {
                key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0", true);
                key.SetValue("Game Speed", speed);
            }

            Debug.WriteLine("launched");

            return process;
        }

        private void InstallPlayer(Player player)
        {
            var src = player.Folder;
            var dest = AiFolder;

            var stack = new Stack<string>();
            stack.Push(src);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var other = current.Replace(src, dest);
                if (!Directory.Exists(other))
                {
                    Directory.CreateDirectory(other);
                }

                foreach (var file in Directory.EnumerateFiles(current).Where(f => Path.GetExtension(f) == ".per" || Path.GetExtension(f) == ".ai"))
                {
                    var dest_file = Path.Combine(other, Path.GetFileName(file));
                    File.Copy(file, dest_file, true);
                }

                foreach (var dir in Directory.EnumerateDirectories(current))
                {
                    stack.Push(dir);
                }
            }
        }

        private MessagePackObject Call(string method, params object[] arguments)
        {
            if (RpcClient == null)
            {
                throw new Exception("rpc client == null");
            }

            if (!RpcClient.IsConnected)
            {
                throw new Exception("RcpClient not connected");
            }

            var task = RpcClient.CallAsync(method, arguments, null);
            var sw = new Stopwatch();
            sw.Start();
            while (!task.IsCompleted)
            {
                Thread.Sleep(100);
                if (sw.ElapsedMilliseconds > 100 * 1000)
                {
                    break;
                }
            }

            if (!task.IsCompletedSuccessfully)
            {
                throw new Exception("call not returned from rpc client");
            }

            return task.Result;
        }
    }
}
