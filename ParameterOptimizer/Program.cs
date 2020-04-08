using System;
using System.Collections.Generic;
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

            var meleon = new Participant("Meleon", new List<int>() { 3, 5, 15, 16 });
            var uly = new Participant("UlyssesWK", new List<int>() { 19 });
            var strongbow = new Participant("Strong Bow v1b", new List<int>() { 16 });
            var kosmos = new Participant("Kosmos3.00beta2", new List<int>() { 19 });
            var celtic = new Participant("InFamous-Celtic", new List<int>() { 13 });

            var opponents = new List<Participant>() { meleon, uly, strongbow, kosmos, celtic };

            var target_army_escrow = new Parameter("target-army-escrow", 1, 9);

            var parameters = new List<Parameter>() { target_army_escrow };

            var games = 3;
            var maps = new List<int>() { 9 };

            Optimizer.Run(exe, parameters, ai, opponents, games, maps);
        }
    }
}
