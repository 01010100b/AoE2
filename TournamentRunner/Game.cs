using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentRunner
{
    public class Game
    {
        private volatile bool _Finished = false;
        public bool Finished { get { return _Finished; } internal set { _Finished = value; } }
        public readonly int GameType;
        public readonly int MapType;
        public readonly int MapSize;
        public readonly bool Record;
        public readonly List<Player> Players;

        // undefined before Finished = true
        private readonly List<int> _Winners = new List<int>();
        public List<int> Winners { get { return GetWinners(); } }
        public List<int> WinningTeams { get { return GetWinningTeams(); } }
        public bool Draw { get { return GetDraw(); } }

        public Game(int game_type, int map_type, int map_size, List<Player> players, bool record)
        {
            GameType = game_type;
            MapType = map_type;
            MapSize = map_size;
            Players = players.ToList();
            Record = record;
        }

        internal void SetWinners(List<int> winners)
        {
            _Winners.Clear();
            _Winners.AddRange(winners);
        }

        private List<int> GetWinners()
        {
            if (!Finished)
            {
                throw new Exception("Game isn't finished yet");
            }

            return _Winners;
        }

        private List<int> GetWinningTeams()
        {
            if (!Finished)
            {
                throw new Exception("Game isn't finished yet");
            }

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

        private bool GetDraw()
        {
            if (!Finished)
            {
                throw new Exception("Game isn't finished yet");
            }

            return Winners.Count == 0;
        }
    }
}
