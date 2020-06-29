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
            var ladder = new Ladder();
            ladder.Run();
        }
    }
}
