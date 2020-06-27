using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var exe = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "Age2_x1", "age2_x1.5.exe");
            if (!File.Exists(exe))
            {
                throw new Exception("File not found: " + exe);
            }

            var ai_folder = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "Ai");
            if (!Directory.Exists(ai_folder))
            {
                throw new Exception("Folder not found: " + ai_folder);
            }

            var rec_folder = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "SaveGame");
            if (!Directory.Exists(rec_folder))
            {
                throw new Exception("Folder not found: " + rec_folder);
            }

            var runner = new Runner(exe, 100, ai_folder, rec_folder);
            
            runner.Startup();
            Debug.WriteLine("startup done");

            var binary = new Player("Binary", 0, 19, null);
            var barbarian = new Player("Barbarian", 0, 19, null);

            var game = new Game(binary, barbarian);
            var result = runner.Run(game);

            foreach (var winner in result.Winners)
            {
                Debug.WriteLine("Winner: " + winner.Name);
            }

            Debug.WriteLine("Rec size: " + result.Rec?.Length / 1000000d + "mb");
        }
    }
}
