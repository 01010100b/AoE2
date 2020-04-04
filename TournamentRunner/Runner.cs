using Microsoft.Win32;
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
        private static readonly Queue<Match> Matches = new Queue<Match>();

        public static void Run(string exe, int speed, List<Match> games)
        {
            Matches.Clear();

            foreach (var game in games)
            {
                Matches.Enqueue(game);
            }

            var old_speed = GetSpeed();
            SetSpeed(speed);

            var rng = new Random();
            var instances = new List<Thread>();
            var num_instances = Math.Min(6, Matches.Count);

            for (int i = 0; i < num_instances; i++)
            {
                var port = rng.Next(40000, 65000);
                var aoc = Launch(exe, port);
                var inst = new Thread(() => RunInstance(aoc, port))
                {
                    IsBackground = true
                };

                instances.Add(inst);
                inst.Start();

                Thread.Sleep(2 * 1000);
            }

            SetSpeed(old_speed);

            foreach (var inst in instances)
            {
                inst.Join();
            }
        }

        private static void RunInstance(Process aoc, int port)
        {
            lock (Matches)
            {
                if (Matches.Count == 0)
                {
                    aoc.Kill();
                    return;
                }
            }

            while (true)
            {
                Match game = null;
                lock (Matches)
                {
                    if (Matches.Count > 0)
                    {
                        game = Matches.Dequeue();
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

        private static void RunGame(Match game, int port)
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

                foreach (var player in game.Players)
                {
                    args += " " + player.Name.Replace(" ", "%20");
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
                        game.Winners.AddRange(winners);
                    }
                }
            }

            lock (game)
            {
                game.Finished = true;
            }

            Debug.WriteLine("done running");
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
