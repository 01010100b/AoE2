using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TournamentRunner;
using static TournamentRunner.Tournament;

namespace ParameterOptimizer
{
    public static class Optimizer
    {
        public static void Run(string exe, List<Parameter> parameters, Participant ai, List<Participant> opponents, int games, List<int> maps)
        {
            Runner.Startup(exe, 100);

            var folder = Path.GetDirectoryName(exe);
            var parent = Directory.GetParent(folder);
            var ai_folder = Path.Combine(parent.FullName, "Ai");
            var ai_file = Path.Combine(ai_folder, ai.Name + ".per");

            var best = new Dictionary<string, int>();
            foreach (var parameter in parameters)
            {
                best[parameter.Name] = (parameter.Min + parameter.Max) / 2;
            }

            var result_file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "best.txt");
            if (File.Exists(result_file))
            {
                foreach (var line in File.ReadAllLines(result_file))
                {
                    var pieces = line.Split('=');
                    var name = pieces[0];
                    var val = int.Parse(pieces[1]);

                    best[name] = val;
                }
            }

            foreach (var parameter in parameters)
            {
                var best_score = -1;

                Debug.WriteLine("Starting parameter: " + parameter.Name);
                var individuals = new List<Individual>();

                for (int v = parameter.Min; v <= parameter.Max; v++)
                {
                    var ind = new Individual();
                    foreach (var kvp in best)
                    {
                        ind.Parameters[kvp.Key] = kvp.Value;
                    }

                    ind.Parameters[parameter.Name] = v;

                    individuals.Add(ind);
                }

                foreach (var individual in individuals)
                {
                    Debug.Write("Starting individual...");

                    SetParameters(ai_file, individual.Parameters);
                    Thread.Sleep(2 * 1000);

                    var score = GetScore(ai, opponents, games, maps);
                    Debug.WriteLine("score: " + score);
                    if (score > best_score)
                    {
                        best[parameter.Name] = individual.Parameters[parameter.Name];
                        Debug.WriteLine($"New best value: {parameter.Name} = {best[parameter.Name]}");

                        var lines = new List<string>();
                        foreach (var kvp in best)
                        {
                            lines.Add(kvp.Key + "=" + kvp.Value);
                        }

                        lines.Sort();
                        File.WriteAllLines(result_file, lines);

                        best_score = score;
                    }

                    Thread.Sleep(5 * 1000);
                }
            }

            SetParameters(ai_file, best);

            Runner.Shutdown();
        }

        private static int GetScore(Participant ai, List<Participant> opponents, int games, List<int> maps)
        {
            var teams = new List<int>() { 1, 2, 4 };

            var max_score = 2 * games * teams.Count * opponents.Count;

            var particpants = opponents.ToList();
            particpants.Insert(0, ai);
            var tournament = new Tournament(games, maps, particpants, teams, false);

            Runner.Enqueue(tournament.Matches);

            var finished = false;
            while (!finished)
            {
                finished = true;

                foreach (var match in tournament.Matches)
                {
                    if (!match.Finished)
                    {
                        finished = false;
                        break;
                    }
                }

                Thread.Sleep(2 * 1000);
            }

            var score = 0;
            foreach (var match in tournament.Matches)
            {
                var team1 = match.Players.First(p => p.Team == 1).Name;
                var team2 = match.Players.First(p => p.Team == 2).Name;

                var opponent = team1;
                if (opponent.Equals(ai.Name))
                {
                    opponent = team2;
                }

                if (match.Draw)
                {
                    score += 1;
                }
                else
                {
                    var my_team = 1;
                    if (ai.Name.Equals(team2))
                    {
                        my_team = 2;
                    }

                    if (match.WinningTeams.Contains(my_team))
                    {
                        score += 2;
                    }
                }
            }

            score *= 100;
            score /= max_score;

            return score;
        }

        private static void SetParameters(string ai_file, Dictionary<string, int> parameters)
        {
            var lines = File.ReadAllLines(ai_file);

            foreach (var kvp in parameters)
            {
                var search = "(defconst " + kvp.Key + " ";
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(search))
                    {
                        lines[i] = "(defconst " + kvp.Key + " " + kvp.Value + ")";
                    }
                }
            }

            File.WriteAllLines(ai_file, lines);
        }
    }
}
