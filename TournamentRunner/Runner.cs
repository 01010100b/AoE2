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
    class Runner
    {
        private static readonly Queue<Game> Games = new Queue<Game>();

        public static void Run(IEnumerable<Game> games)
        {
            Games.Clear();

            foreach (var game in games)
            {
                Games.Enqueue(game);
            }

            var rng = new Random();
            var instances = new List<Thread>();

            for (int i = 0; i < Math.Min(6, Games.Count); i++)
            {
                var port = rng.Next(40000, 65000);
                var aoc = Launcher.Launch(port);
                var inst = new Thread(() => RunInstance(aoc, port))
                {
                    IsBackground = true
                };

                instances.Add(inst);
                inst.Start();
            }

            foreach (var inst in instances)
            {
                inst.Join();
            }
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

            aoc.Kill();
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
    }
}
