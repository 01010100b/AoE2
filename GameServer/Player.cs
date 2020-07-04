using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    public class Player
    {
        public enum Civs : int
        {
            Random = 19,
            Britons = 1,
            Franks = 2,
            Goths = 3,
            Teutons = 4,
            Japanese = 5,
            Chinese = 6,
            Byzantine = 7,
            Persians = 8,
            Saracens = 9,
            Turks = 10,
            Vikings = 11,
            Mongols = 12,
            Celts = 13,
            Spanish = 14,
            Aztec = 15,
            Mayan = 16,
            Huns = 17,
            Koreans = 18
        }

        public string Name { get; set; }
        public int Team { get; set; }
        public Civs Civ { get; set; }
        public string Folder { get; set; }
    }
}
