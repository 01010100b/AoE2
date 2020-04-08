using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TournamentRunner
{
    public class Tournament
    {
        public class Participant
        {
            public readonly string Name;
            public readonly List<int> Civilizations;

            public Participant(string name, List<int> civs)
            {
                Name = name;
                Civilizations = civs.ToList();
            }

            public override string ToString()
            {
                var str = Name + ", civs:";

                foreach (var civ in Civilizations)
                {
                    str += " " + civ;
                }

                return str;
            }
        }

        public readonly int GamesPerMatch;
        public readonly List<int> Maps;
        public readonly List<Participant> Participants;
        public readonly List<int> TeamSizes;
        public readonly bool Record;
        public readonly List<Game> Games;

        public Tournament(int games, List<int> maps, List<Participant> participants, List<int> teams, bool record)
        {
            GamesPerMatch = games;
            Maps = maps.ToList();
            Participants = participants.ToList();
            TeamSizes = teams.ToList();
            Record = record;

            Games = GetAllGames();
        }

        public void Run(string exe, int speed)
        {
            Runner.Startup(exe, speed);

            Runner.Enqueue(Games);

            var finished = false;
            while (!finished)
            {
                finished = true;
                foreach (var game in Games)
                {
                    if (!game.Finished)
                    {
                        finished = false;
                        break;
                    }
                }

                Thread.Sleep(5 * 1000);
            }

            Runner.Shutdown();
        }

        public List<string> GetResultString()
        {
            var name = Participants[0].Name;
            var wins = new Dictionary<string, int>();
            var losses = new Dictionary<string, int>();
            var draws = new Dictionary<string, int>();

            for (int i = 1; i < Participants.Count; i++)
            {
                wins.Add(Participants[i].Name, 0);
                losses.Add(Participants[i].Name, 0);
                draws.Add(Participants[i].Name, 0);
            }

            lock (Games)
            {
                foreach (var game in Games)
                {
                    if (game.Finished)
                    {
                        var team1 = game.Players.First(p => p.Team == 1).Name;
                        var team2 = game.Players.First(p => p.Team == 2).Name;
                        //Debug.WriteLine("team1: " + team1);
                        //Debug.WriteLine("team2: " + team2);

                        var opponent = team1;
                        if (opponent.Equals(name))
                        {
                            opponent = team2;
                        }

                        //Debug.WriteLine("opponent: " + opponent);

                        if (game.Draw)
                        {
                            draws[opponent]++;
                        }
                        else
                        {
                            var my_team = 1;
                            if (name.Equals(team2))
                            {
                                my_team = 2;
                            }

                            if (game.WinningTeams.Contains(my_team))
                            {
                                wins[opponent]++;
                            }
                            else
                            {
                                losses[opponent]++;
                            }
                        }
                    }
                }
            }
            
            var results = new List<string>();

            var opponents = Participants.Select(p => p.Name).ToList();
            opponents.RemoveAt(0);
            opponents.Sort();

            foreach (var o in opponents)
            {
                var w = wins[o];
                var l = losses[o];
                var d = draws[o];

                var result = $"{name} - {o}: {w} {l} {d}";
                results.Add(result);
            }

            results.Sort();

            return results;
        }

        private List<Game> GetAllGames()
        {
            var games = new List<Game>();
            var rng = new Random();

            for (int i = 1; i < Participants.Count; i++)
            {
                games.AddRange(GetGames(rng, Participants[0], Participants[i]));
            }

            return games.OrderBy(m => rng.Next()).ToList();
        }

        private List<Game> GetGames(Random rng, Participant a, Participant b)
        {
            var games = new List<Game>();

            foreach (var teamsize in TeamSizes)
            {
                var mapsize = teamsize;
                if (teamsize == 1)
                {
                    mapsize = 0;
                }

                for (int game = 0; game < GamesPerMatch; game++)
                {
                    var players = new List<Player>();

                    for (int i = 0; i < teamsize; i++)
                    {
                        var name = a.Name;
                        var civ = a.Civilizations[rng.Next(a.Civilizations.Count)];

                        players.Add(new Player(name, 1, civ));
                    }

                    for (int i = 0; i < teamsize; i++)
                    {
                        var name = b.Name;
                        var civ = b.Civilizations[rng.Next(b.Civilizations.Count)];

                        players.Add(new Player(name, 2, civ));
                    }

                    games.Add(new Game(0, Maps[rng.Next(Maps.Count)], mapsize, players, Record));
                }
            }

            return games;
        }
    }
}
