using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentRunner
{
    public class Game
    {
        public bool Finished { get; internal set; } = false;
        public readonly int GameType;
        public readonly int MapType;
        public readonly int MapSize;
        public readonly List<Player> Players;
        public readonly List<int> Winners = new List<int>();

        public Game(int game_type, int map_type, int map_size, List<Player> players)
        {
            GameType = game_type;
            MapType = map_type;
            MapSize = map_size;
            Players = players.ToList();
        }
    }
}
