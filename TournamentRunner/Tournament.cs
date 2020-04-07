using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentRunner
{
    class Tournament
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
        }

        public readonly int Games;
        public readonly List<int> Maps;
        public readonly List<Participant> Participants;
        public readonly List<int> TeamSizes;
        public readonly bool Record;
        public readonly List<Match> Matches;

        public Tournament(int games, List<int> maps, List<Participant> participants, List<int> teams, bool record)
        {
            Games = games;
            Maps = maps.ToList();
            Participants = participants.ToList();
            TeamSizes = teams.ToList();
            Record = record;

            Matches = GetAllMatches();
        }

        public void Run(string exe, int speed)
        {
            Runner.Startup(exe, speed);
            Runner.Run(Matches);
            Runner.WaitForFinish();
            Runner.Shutdown();
        }

        private List<Match> GetAllMatches()
        {
            var matches = new List<Match>();
            var rng = new Random();

            for (int i = 1; i < Participants.Count; i++)
            {
                matches.AddRange(GetMatches(rng, Participants[0], Participants[i]));
            }

            return matches.OrderBy(m => rng.Next()).ToList();
        }

        private List<Match> GetMatches(Random rng, Participant a, Participant b)
        {
            var matches = new List<Match>();

            foreach (var teamsize in TeamSizes)
            {
                var mapsize = teamsize;
                if (teamsize == 1)
                {
                    mapsize = 0;
                }

                for (int game = 0; game < Games; game++)
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

                    matches.Add(new Match(0, Maps[rng.Next(Maps.Count)], mapsize, players, Record));
                }
            }

            return matches;
        }
    }
}
