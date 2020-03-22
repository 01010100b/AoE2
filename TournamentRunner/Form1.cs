using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TournamentRunner
{
    public partial class Form1 : Form
    {
        private Thread RunThread { get; set; } = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonLaunch_Click(object sender, EventArgs e)
        {
            ButtonLaunch.Enabled = false;
            var folder = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "Age2_x1");
            if (!Directory.Exists(folder))
            {
                var diag = new OpenFileDialog
                {
                    Filter = "aoe2|age2_x1.exe"
                };
                diag.ShowDialog();

                var file = diag.FileName;
                folder = Path.GetDirectoryName(file);
            }

            RichOutput.Clear();
            RichOutput.AppendText("Running games...\n");

            RunThread = new Thread(() => Run(folder))
            {
                IsBackground = true
            };
            RunThread.Start();
        }

        private void Run(string folder)
        {
            var sw = new Stopwatch();
            sw.Start();

            var number_of_games = int.Parse(TextNumberGames.Text);
            var game_type = int.Parse(TextGameType.Text);
            var map_type = int.Parse(TextMapType.Text);
            var map_size = int.Parse(TextMapSize.Text);

            var players = new List<Tuple<string, int, int>>()
            {
                new Tuple<string, int, int>(TextName1.Text, int.Parse(TextTeam1.Text), int.Parse(TextCiv1.Text)),
                new Tuple<string, int, int>(TextName2.Text, int.Parse(TextTeam2.Text), int.Parse(TextCiv2.Text)),
                new Tuple<string, int, int>(TextName3.Text, int.Parse(TextTeam3.Text), int.Parse(TextCiv3.Text)),
                new Tuple<string, int, int>(TextName4.Text, int.Parse(TextTeam4.Text), int.Parse(TextCiv4.Text)),
                new Tuple<string, int, int>(TextName5.Text, int.Parse(TextTeam5.Text), int.Parse(TextCiv5.Text)),
                new Tuple<string, int, int>(TextName6.Text, int.Parse(TextTeam6.Text), int.Parse(TextCiv6.Text)),
                new Tuple<string, int, int>(TextName7.Text, int.Parse(TextTeam7.Text), int.Parse(TextCiv7.Text)),
                new Tuple<string, int, int>(TextName8.Text, int.Parse(TextTeam8.Text), int.Parse(TextCiv8.Text)),
            };

            var games = new List<Game>();

            for (int i = 0; i < number_of_games; i++)
            {
                var game = new Game(game_type, map_type, map_size, players.Select(t => new Player(t.Item1, t.Item2, t.Item3)).ToList());
                games.Add(game);
            }

            var runner = new Thread(() => Runner.Run(folder, games))
            {
                IsBackground = true
            };
            runner.Start();

            var finished = 0;

            while (finished < games.Count)
            {
                var current_finished = 0;
                var wins = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };

                foreach (var game in games)
                {
                    lock (game)
                    {
                        if (game.Finished)
                        {
                            current_finished++;

                            foreach (var winner in game.Winners)
                            {
                                wins[winner - 1]++;
                            }
                        }
                    }
                }

                if (current_finished > finished)
                {
                    finished = current_finished;
                    var result = "Result after game " + finished + "/" + games.Count + ":";
                    for (int i = 0; i < 8; i++)
                    {
                        result += " " + wins[i];
                    }

                    Invoke(new Action(() => RichOutput.AppendText(result + "\n")));

                }

                Thread.Sleep(5 * 1000);
            }

            runner.Join();
            
            sw.Stop();
            var time = "Took " + sw.Elapsed.TotalMinutes.ToString("N2") + " m, " + (sw.Elapsed.TotalMinutes / games.Count).ToString("N2") + " m/game";
            Invoke(new Action(() => RichOutput.AppendText(time + "\n")));

            Invoke(new Action(() => ButtonLaunch.Enabled = true));
        }
    }
}
