using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentRunner
{
    public class Player
    {
        public readonly string Name;
        public readonly int Team;
        public readonly int Civ;

        public Player(string name, int team, int civ)
        {
            Name = name;
            Team = team;
            Civ = civ;
        }
    }
}
