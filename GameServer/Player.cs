using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    public class Player
    {
        public string Name { get; set; }
        public int Team { get; set; }
        public Civ Civ { get; set; }
        public string Folder { get; set; }
    }
}
