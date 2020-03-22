using Reloaded.Injector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TournamentRunner
{
    class Launcher
    {
        public static Process Launch(int port)
        {
            var dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aoc-auto-game.dll");
            var aoc_name = "age2_x1.5.exe";
            var exe = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "Age2_x1", aoc_name);

            var process = Process.Start(exe, "-multipleinstances -autogameport " + port);
            Thread.Sleep(2 * 1000);

            using (var injector = new Injector(process))
            {
                injector.Inject(dll);
            }

            Thread.Sleep(10 * 1000);
            Debug.WriteLine("launched");

            return process;
        }
    }
}
