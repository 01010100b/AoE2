using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentRunner
{
    public class Player
    {
        public static List<string> GetPlayers(string exe)
        {
            var folder = Path.GetDirectoryName(exe);
            var parent = Directory.GetParent(folder);
            var ai_folder = Path.Combine(parent.FullName, "Ai");

            return Directory.EnumerateFiles(ai_folder, "*.ai").Select(a => Path.GetFileNameWithoutExtension(a)).ToList();
        }


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
