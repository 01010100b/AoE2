using Reloaded.Injector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentRunner
{
    class Launcher
    {
        public static void Launch()
        {
            var dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aoc-auto-game.dll");
            var aoc_name = "age2_x1.5.exe";
            var exe = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), @"Microsoft Games\Age of Empires ii\Age2_x1\" + aoc_name);

            foreach (var p in Process.GetProcessesByName(aoc_name))
            {
                p.Kill();
            }

            var process = Process.Start(exe);
            
            using (var injector = new Injector(process))
            {
                injector.Inject(dll);
            }
        }
    }
}
