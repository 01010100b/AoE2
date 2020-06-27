using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class Game
    {
        public List<Player> Players = new List<Player>();
        
        public int GameType = 0;
        public string ScenarioName = null;
        public int MapType = 9;
        public int MapSize = 0;
        public int Difficulty = 1;
        public int StartingResources = 0;
        public int PopulationLimit = 200;
        public int RevealMap = 0;
        public int StartingAge = 0;
        public int VictoryType = 0;
        public int VictoryValue = 0;
        public bool TeamsTogether = true;
        public bool LockTeams = true;
        public bool AllTechs = true;
        public bool Recorded = true;

        public Game(params Player[] players)
        {
            Players.AddRange(players);
        }
    }
}
