using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameServer
{
    public class Settings
    {
        public string Exe { get; set; } = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "Age2_x1", "age2_x1.5.exe");
        public int Speed { get; set; } = 100;
        public string AiFolder { get; set; } = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "Ai");
        public string RecFolder { get; set; } = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "SaveGame");
    }
}
