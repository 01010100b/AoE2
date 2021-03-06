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
        private static readonly List<Thread> Instances = new List<Thread>();
        private static volatile bool Stop = false;
        private static int Speed = 100;

        public static void Startup(string exe, int speed)
        {
            Shutdown();

            var num_instances = (3 * Environment.ProcessorCount) / 2;
            if (num_instances < 1)
            {
                num_instances = 1;
            }

            Speed = speed;
            var rng = new Random();
            var ports = new List<int>();
            while (ports.Count < num_instances)
            {
                var p = rng.Next(40000, 65000);
                while (ports.Contains(p))
                {
                    p = rng.Next(40000, 65000);
                }

                ports.Add(p);
            }

            for (int i = 0; i < num_instances; i++)
            {
                var port = ports[i];
                var aoc = Launch(exe, port);
                var inst = new Thread(() => RunInstance(aoc, port))
                {
                    IsBackground = true
                };

                Instances.Add(inst);
                inst.Start();
            }
        }

        public static void Enqueue(IEnumerable<Game> games) 
        {
            lock (Games)
            {
                foreach (var game in games.Where(m => !m.Finished))
                {
                    Games.Enqueue(game);
                }
            }
        }

        public static void WaitForGames(IEnumerable<Game> games)
        {
            foreach (var game in games)
            {
                while (!game.Finished)
                {
                    Thread.Sleep(2 * 1000);
                }
            }
        }

        public static void Shutdown()
        {
            Stop = true;

            foreach (var instance in Instances)
            {
                instance.Join();
            }

            Thread.Sleep(10 * 1000);

            Games.Clear();
            Instances.Clear();
            Stop = false;
        }

        private static void RunInstance(Process aoc, int port)
        {
            while (!Stop)
            {
                Game game = null;
                lock (Games)
                {
                    if (Games.Count > 0)
                    {
                        game = Games.Dequeue();
                    }
                }

                if (game != null)
                {
                    var result = false;
                    
                    try
                    {
                        result = RunGame(game, port);
                    }
                    catch (Exception e)
                    {
                        result = false;
                        Debug.WriteLine(e.Message);
                    }

                    if (aoc.HasExited)
                    {
                        result = false;
                    }

                    if (result)
                    {
                        game.Finished = true;
                    }
                    else
                    {
                        Debug.WriteLine("Error: failed result at " + DateTime.Now.ToShortTimeString());

                        lock (Games)
                        {
                            Games.Enqueue(game);
                        }

                        var launched = false;
                        do
                        {
                            if (!aoc.HasExited)
                            {
                                aoc.Kill();
                                aoc.WaitForExit();
                            }

                            if (Stop)
                            {
                                break;
                            }

                            try
                            {
                                Thread.Sleep(10 * 1000);
                                aoc = Launch(aoc.StartInfo.FileName, port);
                                launched = true;
                            }
                            catch (Exception e)
                            {
                                launched = false;
                                Debug.WriteLine("Exception: " + e.Message);
                            }

                        } while (!launched);
                    }
                }

                Thread.Sleep(1 * 1000);
            }

            if (!aoc.HasExited)
            {
                aoc.Kill();
                aoc.WaitForExit();
            }
        }

        private static bool RunGame(Game game, int port)
        {
            var cmd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runner", "GameRunner.exe");
            if (!File.Exists(cmd))
            {
                throw new Exception("File not found: " + cmd);
            }

            var args = port.ToString();

            lock (game)
            {
                args += " " + game.GameType;
                args += " " + game.MapType;
                args += " " + game.MapSize;
                args += " " + (game.Record ? "true" : "false");

                for (int i = 0; i < 8; i++)
                {
                    if (i < game.Players.Count)
                    {
                        var player = game.Players[i];

                        args += " " + player.Name.Replace(" ", "%20");
                        args += " " + player.Team;
                        args += " " + player.Civ;
                    }
                    else
                    {
                        args += " Closed 0 0";
                    }
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
            
            lock (Instances)
            {
                process.Start();
                Thread.Sleep(10 * 1000);
            }
            
            var gotresult = false;
            while (!process.HasExited)
            {
                var line = process.StandardOutput.ReadLine();

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
                        game.SetWinners(winners);
                        gotresult = true;
                    }
                }
            }

            return gotresult;
        }

        private static Process Launch(string exe, int port)
        {
            Debug.WriteLine("start launching");

            var dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aoc-auto-game.dll");
            if (!File.Exists(dll))
            {
                throw new Exception("File not found: " + dll);
            }
            
            if (!File.Exists(exe))
            {
                throw new Exception("File not found: " + exe);
            }

            lock (Instances)
            {
                var old_speed = GetSpeed();
                SetSpeed(Speed);

                Process process = null;
                try
                {
                    process = Process.Start(exe, "-multipleinstances -autogameport " + port);

                    while (!process.Responding)
                    {
                        Debug.WriteLine("Waiting for process to respond");
                        Thread.Sleep(1 * 1000);
                    }

                    Thread.Sleep(3 * 1000);
                    using (var injector = new Injector(process))
                    {
                        injector.Inject(dll);
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
                    SetSpeed(old_speed);
                }

                Debug.WriteLine("launched");

                return process;
            }
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
