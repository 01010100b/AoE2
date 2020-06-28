using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    public class GameResult
    {
        public readonly Game Game;
        public readonly bool Crashed = false;
        public readonly List<Player> Winners = new List<Player>();
        public byte[] Rec { get; internal set; } = null;

        public GameResult(Game game, bool crashed)
        {
            Game = game;
            Crashed = crashed;
        }
    }
}
