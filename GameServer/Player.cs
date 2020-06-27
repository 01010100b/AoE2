using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class Player
    {
        public readonly string Name;
        public readonly int Team;
        public readonly int Civ;
        public readonly string Folder;

        public Player(string name, int team, int civ, string folder)
        {
            Name = name;
            Team = team;
            Civ = civ;
            Folder = folder;
        }
    }
}
