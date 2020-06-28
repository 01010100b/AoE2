using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace GameServer
{
    class Program
    {
        private static Ladder Ladder;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            Ladder = new Ladder();
            Ladder.Run();
        }

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Debug.WriteLine("unhandled exception: " + args.ExceptionObject);
            Ladder.Reset = true;
        }
    }
}
