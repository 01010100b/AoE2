﻿using Microsoft.Win32;
using Reloaded.Injector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TournamentRunner
{
    public static class Runner
    {
        private static readonly Queue<Game> Games = new Queue<Game>();

        public static void Run(List<Game> games)
        {
            Games.Clear();

            foreach (var game in games)
            {
                Games.Enqueue(game);
            }

            var speed = GetSpeed();
            SetSpeed(150);
            Debug.WriteLine("speed: " + speed);

            var rng = new Random();
            var instances = new List<Thread>();

            for (int i = 0; i < Math.Min(6, games.Count); i++)
            {
                var port = rng.Next(40000, 65000);
                var aoc = Launch(port);
                var inst = new Thread(() => RunInstance(aoc, port))
                {
                    IsBackground = true
                };

                instances.Add(inst);
                inst.Start();

                Thread.Sleep(2 * 1000);
            }

            foreach (var inst in instances)
            {
                inst.Join();
            }

            SetSpeed(speed);
        }

        private static void RunInstance(Process aoc, int port)
        {
            lock (Games)
            {
                if (Games.Count == 0)
                {
                    aoc.Kill();
                    return;
                }
            }

            while (true)
            {
                Game game = null;
                lock (Games)
                {
                    if (Games.Count > 0)
                    {
                        game = Games.Dequeue();
                    }
                }

                if (game == null)
                {
                    break;
                }

                RunGame(game, port);
            }

            if (!aoc.HasExited)
            {
                aoc.Kill();
            }
        }

        private static void RunGame(Game game, int port)
        {
            var cmd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runner", "GameRunner.exe");
            var args = port.ToString();

            lock (game)
            {
                args += " " + game.GameType;
                args += " " + game.MapType;
                args += " " + game.MapSize;

                foreach (var player in game.Players)
                {
                    args += " " + player.Name;
                    args += " " + player.Team;
                    args += " " + player.Civ;
                }
            }
            
            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = cmd,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            var process = new Process()
            {
                StartInfo = info
            };
            process.Start();

            while (!process.HasExited)
            {
                var line = process.StandardOutput.ReadLine();
                Debug.WriteLine(line);

                if (line != null && line.StartsWith("Result"))
                {
                    var pieces = line.Split(' ');
                    var winners = new List<int>();

                    for (int i = 1; i <= 8; i++)
                    {
                        if (pieces[i] == "1")
                        {
                            winners.Add(i);
                        }
                    }

                    lock (game)
                    {
                        game.Winners = winners.ToList();
                    }
                }
            }

            lock (game)
            {
                game.Finished = true;
            }

            Debug.WriteLine("done running");
        }

        private static Process Launch(int port)
        {
            Debug.WriteLine("start launching");
            var dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aoc-auto-game.dll");
            var aoc_name = "age2_x1.5.exe";
            var exe = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "Age2_x1", aoc_name);

            var process = Process.Start(exe, "-multipleinstances -autogameport " + port);
            while (!process.Responding)
            {
                Thread.Sleep(1 * 1000);
                Debug.WriteLine("Waiting for process to respond");
            }

            using (var injector = new Injector(process))
            {
                injector.Inject(dll);
            }

            Thread.Sleep(10 * 1000);
            Debug.WriteLine("launched");

            return process;
        }

        private static int GetSpeed()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0");
            return (int)key.GetValue("Game Speed");
        }

        private static void SetSpeed(int speed)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0", true);
            key.SetValue("Game Speed", speed);
        }
    }
}