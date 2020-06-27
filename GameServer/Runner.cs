using Bleak;
using Microsoft.Win32;
using MsgPack.Rpc.Core.Client;
using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace GameServer
{
    class Runner
    {
        public readonly string Exe;
        public readonly int Speed;
        public readonly string AiFolder;
        public readonly string RecFolder;

        private Process Process = null;
        private RpcClient RpcClient = null;

        public Runner(string exe, int speed, string ai_folder, string rec_folder)
        {
            Exe = exe;
            Speed = speed;
            AiFolder = ai_folder;
            RecFolder = rec_folder;
        }

        public void Startup()
        {
            var rng = new Random();
            var port = rng.Next(40000, 60000);
            
            Process = Launch(Exe, port);
            RpcClient = new RpcClient(new IPEndPoint(IPAddress.Loopback, port));

            var api = RpcClient.Call("GetApiVersion");
            Debug.WriteLine("Api: " + api.ToString());
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public GameResult Run(Game game)
        {
            RpcClient.Call("ResetGameSettings");

            RpcClient.Call("SetGameType", game.GameType);
            if (game.GameType == 3)
            {
                RpcClient.Call("SetGameScenarioName", game.ScenarioName);
            }
            
            RpcClient.Call("SetGameMapType", game.MapType);
            RpcClient.Call("SetGameMapSize", game.MapSize);
            RpcClient.Call("SetGameDifficulty", game.Difficulty);
            RpcClient.Call("SetGameStartingResources", game.StartingResources);
            RpcClient.Call("SetGamePopulationLimit", game.PopulationLimit);
            RpcClient.Call("SetGameRevealMap", game.RevealMap);
            RpcClient.Call("SetGameStartingAge", game.StartingAge);
            RpcClient.Call("SetGameVictoryType", game.VictoryType, game.VictoryValue);
            RpcClient.Call("SetGameTeamsTogether", game.TeamsTogether);
            RpcClient.Call("SetGameTeamsLocked", game.LockTeams);
            RpcClient.Call("SetGameAllTechs", game.AllTechs);
            RpcClient.Call("SetGameRecorded", game.Recorded);

            for (int i = 0; i < 8; i++)
            {
                var player = i + 1;
                if (i < game.Players.Count)
                {
                    RpcClient.Call("SetPlayerComputer", player, game.Players[i].Name);
                    RpcClient.Call("SetPlayerCivilization", player, game.Players[i].Civ);
                    RpcClient.Call("SetPlayerTeam", player, game.Players[i].Team);
                }
                else
                {
                    RpcClient.Call("SetPlayerClosed", player);
                }
            }

            RpcClient.Call("StartGame");

            return null;
        }

        private Process Launch(string exe, int port)
        {
            Debug.WriteLine("start launching");

            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0");
            var speed = (int)key.GetValue("Game Speed");

            var dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aoc-auto-game.dll");
            if (!File.Exists(dll))
            {
                throw new Exception("File not found: " + dll);
            }

            if (!File.Exists(exe))
            {
                throw new Exception("File not found: " + exe);
            }

            Process process = null;
            try
            {
                key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0", true);
                key.SetValue("Game Speed", Speed);

                process = Process.Start(exe, "-multipleinstances -autogameport " + port);

                while (!process.Responding)
                {
                    Debug.WriteLine("Waiting for process to respond");
                    Thread.Sleep(1 * 1000);
                }

                Thread.Sleep(3 * 1000);
                using (var injector = new Injector(process.Id, dll, InjectionMethod.CreateThread, InjectionFlags.None))
                {
                    injector.InjectDll();
                }

                Thread.Sleep(10 * 1000);
            }
            catch (Exception e)
            {
                if (process != null && !process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }

                throw e;
            }
            finally
            {
                key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0", true);
                key.SetValue("Game Speed", speed);
            }

            Debug.WriteLine("launched");

            return process;
        }
    }
}
