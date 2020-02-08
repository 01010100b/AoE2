using Binary.Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Binary
{
    class Program
    {
        public const int MAX_GOALS = 512;
        public const int FUNCTION_PARAMETERS = 8;
        public const int MIN_TEMPORARY_VARS = 32;

        static void Main(string[] args)
        {
            var listeners = new TraceListener[] { new TextWriterTraceListener(Console.Out) };
            Trace.Listeners.AddRange(listeners); // write trace to console as well

            Test();

            var sw = new Stopwatch();
            sw.Start();

            var folder = AppDomain.CurrentDomain.BaseDirectory;
            //folder = @"C:\Users\Tim\Desktop\aoe2\LanguageExtensions";
            var ai_file = Directory.EnumerateFiles(Path.Combine(folder, "Script"), "*.ai").First();
            var main_file = Path.GetFileName(ai_file).Replace(".ai", ".per");
            var loaded_files = Load(Path.Combine(folder, "Script"));

            Assert.That(loaded_files.ContainsKey(main_file), "Main file not found: " + main_file);

            Trace.WriteLine("Start parsing\n");
            var parser = new Parser();
            var files = parser.Parse(loaded_files);
            Trace.WriteLine("\nDone parsing\n");

            Trace.WriteLine("Start loading\n");
            var loader = new Loader();
            var loaded = loader.Load(main_file, files);
            Trace.WriteLine("\nDone loading\n");

            Trace.WriteLine("Start compiling\n");
            var compiler = new Compiler();
            compiler.Compile(loaded);
            Trace.WriteLine("\nDone compiling\n");

            files[main_file] = loaded;

            Trace.WriteLine("Rules: " + loaded.Sum(e => e.RuleLength));
            Trace.WriteLine("Free goals: " + loader.FreeGoals);
            Trace.WriteLine("Free strategic numbers: " + compiler.FreeStrategicNumbers);

            // save output

            foreach (var file in files.Keys.ToList())
            {
                if (!(file.Equals(main_file) || file.Contains("GLX-LOAD-RANDOM-RESULT")))
                {
                    files.Remove(file);
                }
            }

            var savefolder = Path.Combine(folder, "ParsedScript");
            if (args.Length > 0)
            {
                savefolder = args[0];
            }
            Trace.WriteLine("Save folder: " + savefolder);

            if (!Directory.Exists(savefolder))
            {
                Directory.CreateDirectory(savefolder);
            }

            var dir = Path.Combine(savefolder, main_file.Replace(".per", ""));
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            var out_ai = Path.Combine(savefolder, Path.GetFileName(ai_file));
            File.Copy(ai_file, out_ai, true);
            Trace.WriteLine("Copied: " + Path.GetFileName(ai_file));

            Save(savefolder, files);

            sw.Stop();
            Trace.WriteLine($"\nTook {sw.Elapsed.TotalSeconds.ToString("F2")} seconds");
            Trace.WriteLine("");

            //Console.ReadLine();
        }

        private static Dictionary<string, string[]> Load(string folder)
        {
            var results = new Dictionary<string, string[]>();
            
            foreach (var file in Directory.EnumerateFiles(folder, "*.per", SearchOption.AllDirectories))
            {
                var lines = File.ReadAllLines(file);
                var rel = file.Replace(folder, "");
                while (rel[0] == Path.DirectorySeparatorChar)
                {
                    rel = rel.Substring(1);
                }
                
                results.Add(rel, lines);
            }

            return results;
        }

        private static void Save(string folder, Dictionary<string, List<Element>> files)
        {
            var kvps = files.ToList();
            kvps.Sort((a, b) => a.Key.CompareTo(b.Key));
            
            foreach (var kvp in kvps)
            {
                var lines = kvp.Value.Select(e => e.Code).ToArray();
                var save_file = Path.Combine(folder, kvp.Key);

                var dir = Path.GetDirectoryName(save_file);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllLines(save_file, lines);

                Trace.WriteLine("Output: " + kvp.Key);
            }
        }

        private static void Test()
        {

        }
    }
}
