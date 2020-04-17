using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TournamentRunner.Tournament;

namespace ParameterOptimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Test();
        }

        private static void Test()
        {
            var exe = @"C:\Users\Tim\AppData\Roaming\Microsoft Games\Age of Empires ii\Age2_x1\age2_x1.5.exe";

            var ai = new Participant("Binary", new List<int>(){ 19 });

            var general = new Participant("The General", new List<int>() { 19 });
            var uly = new Participant("UlyssesWK", new List<int>() { 19 });
            var strongbow = new Participant("Strong Bow v1b", new List<int>() { 16 });
            var kosmos = new Participant("Kosmos3.00beta2", new List<int>() { 19 });
            var celtic = new Participant("InFamous-Celtic", new List<int>() { 13 });

            var opponents = new List<Participant>() { general, uly, strongbow, kosmos, celtic };

            var parameter = new Parameter("use-knight-counter", 0, 1);

            var parameters = new List<Parameter>() { parameter };

            var games = 3;
            var maps = new List<int>() { 9 };

            Optimizer.Run(exe, parameters, ai, opponents, games, maps);

            Console.ReadLine();
        }

        private static void Run()
        {
            string exe = null;
            Participant ai = null;
            int games = -1;
            var opponents = new List<Participant>();
            var parameters = new List<Parameter>();
            var maps = new List<int>();

            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");
            var lines = File.ReadAllLines(file);

            foreach (var line in lines)
            {

            }
        }
    }
}
