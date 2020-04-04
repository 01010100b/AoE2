using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentRunner
{
    public class Match
    {
        public bool Finished { get; internal set; } = false;
        public readonly int GameType;
        public readonly int MapType;
        public readonly int MapSize;
        public readonly List<Player> Players;
        public readonly List<int> Winners = new List<int>();
        public List<int> WinningTeams { get { return GetWinningTeams(); } }
        public bool Draw { get { return Winners.Count == 0; } }

        public Match(int game_type, int map_type, int map_size, List<Player> players)
        {
            GameType = game_type;
            MapType = map_type;
            MapSize = map_size;
            Players = players.ToList();
        }

        private List<int> GetWinningTeams()
        {
            var teams = new List<int>();

            foreach (var w in Winners)
            {
                var p = Players[w - 1];

                if (p.Team > 0 && !teams.Contains(p.Team))
                {
                    teams.Add(p.Team);
                }
            }

            return teams;
        }
    }
}
