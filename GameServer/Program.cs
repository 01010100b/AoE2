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

            var runner = new Runner(exe, 20, null, null);
            
            runner.Startup();
            Debug.WriteLine("startup done");

            var binary = new Player("Binary", 0, 19, null);
            var barbarian = new Player("Barbarian", 0, 19, null);

            var game = new Game(binary, barbarian);
            runner.Run(game);
        }
    }
}
