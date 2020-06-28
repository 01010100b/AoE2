using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    public class Game
    {
        public List<Player> Players { get; set; } = new List<Player>();
        
        public int GameType { get; set; } = 0;
        public string ScenarioName { get; set; } = null;
        public int MapType { get; set; } = 9;
        public int MapSize { get; set; } = 0;
        public int Difficulty { get; set; } = 1;
        public int StartingResources { get; set; } = 0;
        public int PopulationLimit { get; set; } = 200;
        public int RevealMap { get; set; } = 0;
        public int StartingAge { get; set; } = 0;
        public int VictoryType { get; set; } = 1;
        public int VictoryValue { get; set; } = 0;
        public bool TeamsTogether { get; set; } = true;
        public bool LockTeams { get; set; } = true;
        public bool AllTechs { get; set; } = true;
        public bool Recorded { get; set; } = true;
    }
}
