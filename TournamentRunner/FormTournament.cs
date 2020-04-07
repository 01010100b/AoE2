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
using static TournamentRunner.Tournament;

namespace TournamentRunner
{
    public partial class FormTournament : Form
    {
        public FormTournament()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void ButtonLaunch_Click(object sender, EventArgs e)
        {
            ButtonLaunch.Enabled = false;
            SaveSettings();

            var exe = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft Games", "Age of Empires ii", "Age2_x1", "age2_x1.5.exe");
            if (!File.Exists(exe))
            {
                var diag = new OpenFileDialog
                {
                    Filter = "aoe2|age2_x1*.exe"
                };
                diag.ShowDialog();

                exe = diag.FileName;
            }

            RichOutput.Clear();
            RichOutput.Text = "Starting tournament...";

            var games = int.Parse(TextGames.Text);
            var maps = TextMap.Text.Split(',').Select(s => int.Parse(s)).ToList();

            var teams = new List<int>();
            if (Check1v1.Checked)
            {
                teams.Add(1);
            }
            if (Check2v2.Checked)
            {
                teams.Add(2);
            }
            if (Check3v3.Checked)
            {
                teams.Add(3);
            }
            if (Check4v4.Checked)
            {
                teams.Add(4);
            }

            var participants = new List<Participant>();
            
            var name = TextAIName.Text;
            var civs = TextAICivs.Text.Split(',').Select(s => int.Parse(s)).ToList();

            participants.Add(new Participant(name, civs));

            name = TextOpponent1Name.Text;
            if (name.Length > 1 && !name.Equals("Closed"))
            {
                civs = TextOpponent1Civs.Text.Split(',').Select(s => int.Parse(s)).ToList();
                participants.Add(new Participant(name, civs));
            }

            name = TextOpponent2Name.Text;
            if (name.Length > 1 && !name.Equals("Closed"))
            {
                civs = TextOpponent2Civs.Text.Split(',').Select(s => int.Parse(s)).ToList();
                participants.Add(new Participant(name, civs));
            }

            name = TextOpponent3Name.Text;
            if (name.Length > 1 && !name.Equals("Closed"))
            {
                civs = TextOpponent3Civs.Text.Split(',').Select(s => int.Parse(s)).ToList();
                participants.Add(new Participant(name, civs));
            }

            name = TextOpponent4Name.Text;
            if (name.Length > 1 && !name.Equals("Closed"))
            {
                civs = TextOpponent4Civs.Text.Split(',').Select(s => int.Parse(s)).ToList();
                participants.Add(new Participant(name, civs));
            }

            name = TextOpponent5Name.Text;
            if (name.Length > 1 && !name.Equals("Closed"))
            {
                civs = TextOpponent5Civs.Text.Split(',').Select(s => int.Parse(s)).ToList();
                participants.Add(new Participant(name, civs));
            }

            bool record = CheckRecord.Checked;

            var tournament = new Tournament(games, maps, participants, teams, record);

            var runner = new Thread(() => Run(tournament, exe, 100)) { IsBackground = true };
            runner.Start();
        }

        private void Run(Tournament tournament, string exe, int speed)
        {
            var sw = new Stopwatch();
            sw.Start();

            var name = tournament.Participants[0].Name;

            var runner = new Thread(() => tournament.Run(exe, speed));
            runner.Start();

            var finished = 0;
            var wins = new Dictionary<string, int>();
            var losses = new Dictionary<string, int>();
            var draws = new Dictionary<string, int>();

            while (finished < tournament.Matches.Count)
            {
                finished = 0;
                wins.Clear();
                losses.Clear();
                draws.Clear();

                for (int i = 1; i < tournament.Participants.Count; i++)
                {
                    wins.Add(tournament.Participants[i].Name, 0);
                    losses.Add(tournament.Participants[i].Name, 0);
                    draws.Add(tournament.Participants[i].Name, 0);

                    //Debug.WriteLine("participant: " + tournament.Participants[i].Name);
                }

                lock (tournament.Matches)
                {
                    foreach (var match in tournament.Matches)
                    {
                        if (match.Finished)
                        {
                            finished++;

                            var team1 = match.Players.First(p => p.Team == 1).Name;
                            var team2 = match.Players.First(p => p.Team == 2).Name;
                            //Debug.WriteLine("team1: " + team1);
                            //Debug.WriteLine("team2: " + team2);

                            var opponent = team1;
                            if (opponent.Equals(name))
                            {
                                opponent = team2;
                            }

                            //Debug.WriteLine("opponent: " + opponent);

                            if (match.Draw)
                            {
                                draws[opponent]++;
                            }
                            else
                            {
                                var my_team = 1;
                                if (name.Equals(team2))
                                {
                                    my_team = 2;
                                }

                                if (match.WinningTeams.Contains(my_team))
                                {
                                    wins[opponent]++;
                                }
                                else
                                {
                                    losses[opponent]++;
                                }
                            }
                        }
                    }
                }

                var results = new List<string>();

                var opponents = tournament.Participants.Select(p => p.Name).ToList();
                opponents.RemoveAt(0);
                opponents.Sort();

                foreach (var o in opponents)
                {
                    var w = wins[o];
                    var l = losses[o];
                    var d = draws[o];

                    var result = $"{name} - {o}: {w} {l} {d}";
                    results.Add(result);
                }

                results.Sort();

                var sb = new StringBuilder();
                sb.AppendLine($"After game {finished}/{tournament.Matches.Count}");
                foreach (var r in results)
                {
                    sb.AppendLine(r);
                }

                Invoke(new Action(() => RichOutput.Text = sb.ToString()));
                Thread.Sleep(5 * 1000);
            }

            runner.Join();

            sw.Stop();
            var time = "Took " + sw.Elapsed.TotalMinutes.ToString("N2") + " m, " + (sw.Elapsed.TotalMinutes / tournament.Matches.Count).ToString("N2") + " m/game";
            Invoke(new Action(() => RichOutput.AppendText("\n" + time)));

            Invoke(new Action(() => ButtonLaunch.Enabled = true));
        }

        private void LoadSettings()
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tournament.txt");

            if (!File.Exists(file))
            {
                return;
            }

            var lines = File.ReadAllLines(file);
            if (lines.Length != 19)
            {
                File.Delete(file);
                Debug.WriteLine("tournament.txt not correct number of lines");
                return;
            }

            TextMap.Text = lines[0];
            TextGames.Text = lines[1];
            Check1v1.Checked = lines[2].Equals("True") ? true : false;
            Check2v2.Checked = lines[3].Equals("True") ? true : false;
            Check3v3.Checked = lines[4].Equals("True") ? true : false;
            Check4v4.Checked = lines[5].Equals("True") ? true : false;

            TextAIName.Text = lines[6];
            TextAICivs.Text = lines[7];
            TextOpponent1Name.Text = lines[8];
            TextOpponent1Civs.Text = lines[9];
            TextOpponent2Name.Text = lines[10];
            TextOpponent2Civs.Text = lines[11];
            TextOpponent3Name.Text = lines[12];
            TextOpponent3Civs.Text = lines[13];
            TextOpponent4Name.Text = lines[14];
            TextOpponent4Civs.Text = lines[15];
            TextOpponent5Name.Text = lines[16];
            TextOpponent5Civs.Text = lines[17];
            CheckRecord.Checked = lines[18].Equals("True") ? true : false;
        }

        private void SaveSettings()
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tournament.txt");

            var lines = new List<string>()
            {
                TextMap.Text,
                TextGames.Text,
                Check1v1.Checked ? "True" : "False",
                Check2v2.Checked ? "True" : "False",
                Check3v3.Checked ? "True" : "False",
                Check4v4.Checked ? "True" : "False",

                TextAIName.Text,
                TextAICivs.Text,
                TextOpponent1Name.Text,
                TextOpponent1Civs.Text,
                TextOpponent2Name.Text,
                TextOpponent2Civs.Text,
                TextOpponent3Name.Text,
                TextOpponent3Civs.Text,
                TextOpponent4Name.Text,
                TextOpponent4Civs.Text,
                TextOpponent5Name.Text,
                TextOpponent5Civs.Text,
                CheckRecord.Checked ? "True" : "False"
            };

            File.WriteAllLines(file, lines);

        }
    }
}
