using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YTY.AocDatLib;

namespace ScriptCompiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonAnalyze_Click(object sender, EventArgs e)
        {
            var path = (string)Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion\1.0").GetValue("EXE Path");
            path = Path.Combine(path, @"data\empires2_x1_p1.dat");
            Debug.WriteLine("dat: " + path);

            var dat = new DatFile(path);
            var skirms = new List<Unit>();

            foreach (var civ in dat.Civilizations)
            {
                foreach (var unit in civ.Units)
                {
                    if (unit.Id == 7)
                    {
                        skirms.Add(unit);
                    }
                }
            }

            foreach (var skirm in skirms)
            {
                Debug.WriteLine("tech: " + skirm.TechId);
            }
        }
    }
}
