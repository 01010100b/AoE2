using Binary.Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Binary
{
    class Loader
    {
        public int FreeGoals { get; private set; } = 0;

        private string MainFile { get; set; } = null;
        private Dictionary<string, List<Element>> Files { get; set; } = null;
        private Dictionary<string, List<Element>> Functions { get; set; } = new Dictionary<string, List<Element>>();
        private int NextLoadRandomID { get; set; } = 0;

        public List<Element> Load(string main_file, Dictionary<string, List<Element>> files)
        {
            MainFile = main_file;
            Files = files;
            Functions.Clear();
            NextLoadRandomID = 0;

            ExclusiveVariables();
            FunctionDefinitions();
            LoadRandoms();
            
            var loaded = true;
            while (loaded)
            {
                loaded = false;

                Trace.WriteLine("Do loads");
                Loads();

                Trace.WriteLine("Do function calls");
                if (FunctionCalls())
                {
                    loaded = true;
                }
            }

            return files[main_file];
        }

        private void ExclusiveVariables()
        {
            var globals = 0;
            var taken = new HashSet<int>();

            foreach (var elements in Files.Values)
            {
                foreach (var element in elements)
                {
                    if (element is ConstElement e)
                    {
                        if (e.Name.StartsWith("gl-") || e.Name.StartsWith("glx-"))
                        {
                            if (int.TryParse(e.Value, out int value))
                            {
                                taken.Add(value);
                            }
                        }
                    }
                    else if (element.ActualCode.StartsWith("#defglobal"))
                    {
                        globals++;
                    }
                }
            }

            var max_goals_used = 0;
            foreach (var elements in Files.Values)
            {
                var goal_number = 1;
                var goals_used = 0;

                for (int i = 0; i < elements.Count; i++)
                {
                    while (taken.Contains(goal_number))
                    {
                        goal_number++;
                    }

                    var element = elements[i];

                    if (element.ActualCode.StartsWith("#defexclusive"))
                    {
                        var goal = element.Pieces[1];
                        elements[i] = Element.Create($"(defconst {goal} {goal_number})");

                        goal_number++;
                        goals_used++;
                    }
                }

                if (goals_used > max_goals_used)
                {
                    max_goals_used = goals_used;
                }
            }

            var free = Program.MAX_GOALS - taken.Count - globals - max_goals_used;
            const int NEEDED_GOALS = 1 + Program.FUNCTION_PARAMETERS + Program.MIN_TEMPORARY_VARS;

            Assert.That(free >= NEEDED_GOALS, $"Not enough free goals, need at least {NEEDED_GOALS}");

            var new_elements = new List<Element>();
            new_elements.Add(Element.Create("; temporary vars"));
            new_elements.Add(Element.Create(""));
            new_elements.Add(Element.Create("#defglobal glx-return-value"));
            for (int i = 0; i < Program.FUNCTION_PARAMETERS; i++)
            {
                new_elements.Add(Element.Create($"#defglobal glx-param-{i + 1}"));
            }
            new_elements.Add(Element.Create(""));

            free -= 1;
            free -= Program.FUNCTION_PARAMETERS;

            for (int i = 0; i < Math.Min(free, 64); i++)
            {
                new_elements.Add(Element.Create($"#defglobal glx-temporary-{i + 1}"));
            }
            free -= Program.MIN_TEMPORARY_VARS;

            FreeGoals = free;

            Files[MainFile].InsertRange(0, new_elements);
        }

        private bool LoadRandoms()
        {
            var folder = MainFile.Replace(".per", "") + "\\LoadRandoms";
            var loaded = false;

            foreach (var elements in Files.Values.ToList())
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    var element = elements[i];

                    if (element.ActualCode.StartsWith("(load-random") && !element.ActualCode.Contains("GLX-LOAD-RANDOM-RESULT-"))
                    {
                        loaded = true;

                        var name = "GLX-LOAD-RANDOM-RESULT-" + NextLoadRandomID;
                        NextLoadRandomID++;

                        Debug.WriteLine("Parsed load-random " + element.ActualCode);

                        var pieces = element.Pieces;
                        pieces.RemoveAt(0);

                        var files = new List<string>();
                        for (int j = 1; j < pieces.Count; j += 2)
                        {
                            files.Add(pieces[j].Replace(")", "").Replace("\"", "").Trim());
                        }
                        if (pieces.Count % 2 == 1)
                        {
                            files.Add(pieces[pieces.Count - 1].Replace(")", "").Replace("\"", "").Trim());
                        }

                        var code = element.ActualCode;
                        var new_elements = new List<Element>();
                        for (int res = 0; res < files.Count; res++)
                        {
                            var file = files[res];
                            var constant = name + "-" + res;
                            var loadfile = Path.Combine(folder, constant);
                            code = code.Replace(file, loadfile);

                            Files.Add(loadfile + ".per", new List<Element>() { Element.Create($"(defconst {constant} 1)") });

                            new_elements.Add(Element.Create($"#load-if-defined {constant}"));
                            new_elements.Add(Element.Create(""));
                            new_elements.Add(Element.Create($"(load \"{file}\")"));
                            new_elements.Add(Element.Create(""));
                            new_elements.Add(Element.Create("#end-if"));
                        }

                        elements.RemoveAt(i);
                        elements.InsertRange(i, new_elements);
                        elements.Insert(i, Element.Create(code));
                    }
                }
            }

            return loaded;
        }

        private void FunctionDefinitions()
        {
            foreach (var file in Files.Keys)
            {
                var elements = Files[file];

                for (int i = 0; i < elements.Count; i++)
                {
                    var element = elements[i];

                    if (element.ActualCode.StartsWith("#function"))
                    {
                        var pieces = element.Pieces;
                        var function_name = pieces[1];

                        Assert.That(!Functions.ContainsKey(function_name), $"In file {file}: Function already exists: {element.Code}");

                        var function_elements = new List<Element>();

                        function_elements.Add(Element.Create(";" + element.Code));
                        function_elements.Add(Element.Create(""));
                        for (int j = 2; j < pieces.Count; j++)
                        {
                            function_elements.Add(Element.Create("#deflocal " + pieces[j]));
                        }
                        function_elements.Add(Element.Create(""));

                        var setup = new StringBuilder();
                        setup.AppendLine("(defrule ; function setup");
                        setup.AppendLine("\t(true)");
                        setup.AppendLine("=>");
                        for (int j = 2; j < pieces.Count; j++)
                        {
                            setup.AppendLine($"\t(up-modify-goal {pieces[j]} g:= glx-param-{(j - 1).ToString().Trim()})");
                        }
                        setup.Append(")");

                        function_elements.Add(Element.Create(setup.ToString()));

                        elements.RemoveAt(i);

                        while (!elements[i].ActualCode.StartsWith("#return"))
                        {
                            Assert.That(!elements[i].ActualCode.StartsWith("#function"), "Previous function not returned yet: " + elements[i].Code);

                            function_elements.Add(elements[i]);
                            elements.RemoveAt(i);
                        }

                        var goal = elements[i].Pieces[1];

                        function_elements.Add(Element.Create($"(defrule ; return from function\n\t(true)\n=>\n\t(up-modify-goal glx-return-value g:= {goal})\n)"));
                        elements.RemoveAt(i);

                        Functions.Add(function_name, function_elements);

                        Trace.WriteLine("Function: " + function_name);
                    }
                }
            }
        }

        private bool FunctionCalls()
        {
            var called = false;

            var elements = Files[MainFile];

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                var code = element.ActualCode;

                if (code.StartsWith("#call "))
                {
                    // #call func param1 param2 ... ;comment
                    // returns glx-return-value

                    var pieces = element.Pieces;

                    var goal = pieces[1];

                    Assert.That(goal.StartsWith("gl-") || goal.StartsWith("sn-") || goal.Equals("glx-return-value"),
                        $"In file {MainFile}: call result must go into gl-* or sn-*: {element.Code}");

                    var name = pieces[2].Trim();

                    Assert.That(Functions.ContainsKey(name), "Function not found: " + element.Code);

                    //Debug.WriteLine("Inline function call: " + code);

                    var setup = $"(defrule ; set params for {name}\n\t(true)\n=>";
                    for (int p = 3; p < pieces.Count; p++)
                    {
                        var in_par = pieces[p];
                        var par = $"\n\t(up-modify-goal glx-param-{(p - 2).ToString().Trim()} c:= {in_par})";
                        if (in_par.StartsWith("gl-"))
                        {
                            par = $"\n\t(up-modify-goal glx-param-{(p - 2).ToString().Trim()} g:= {in_par})";
                        }
                        else if (in_par.StartsWith("sn-"))
                        {
                            par = $"\n\t(up-modify-goal glx-param-{(p - 2).ToString().Trim()} s:= {in_par})";
                        }

                        setup += par;
                    }
                    setup += "\n)";

                    var ret = $"(defrule ; return from {name}\n\t(true)\n=>\n\t(up-modify-goal {goal} g:= glx-return-value)\n)";
                    if (goal.StartsWith("sn-"))
                    {
                        ret = $"(defrule ; return from {name}\n\t(true)\n=>\n\t(up-modify-sn {goal} g:= glx-return-value)\n)";
                    }

                    elements[i] = Element.Create(";" + elements[i].Code);
                    elements.Insert(i + 1, Element.Create(setup));
                    elements.Insert(i + 2, Element.Create(""));
                    Include(elements, i + 3, Functions[name]);
                    elements.Insert(i + 3 + Functions[name].Count, Element.Create(""));
                    elements.Insert(i + 3 + Functions[name].Count + 1, Element.Create(ret));

                    called = true;
                }
            }

            return called;
        }

        private void Loads()
        {
            var included = true;
            while (included)
            {
                included = false;

                var elements = Files[MainFile];

                for (int i = 0; i < elements.Count; i++)
                {
                    var element = elements[i];
                    if (element is LoadElement le)
                    {
                        Debug.WriteLine("Loaded " + element.ActualCode);

                        var new_elements = new List<Element>();
                        new_elements.Add(Element.Create(";" + string.Concat(Enumerable.Repeat("/", 200))));
                        new_elements.Add(Element.Create("; START FILE: " + le.File));
                        new_elements.Add(Element.Create(""));
                        new_elements.AddRange(Files[le.File]);
                        new_elements.Add(Element.Create(""));
                        new_elements.Add(Element.Create("; END FILE: " + le.File));
                        new_elements.Add(Element.Create(";" + string.Concat(Enumerable.Repeat("/", 200))));

                        elements.RemoveAt(i);
                        Include(elements, i, new_elements);

                        included = true;
                    }
                }
            }
        }

        private void Include(List<Element> parent_elements, int i, List<Element> child_elements)
        {
            child_elements = child_elements.Select(e => Element.Create(e.Code)).ToList();

            var parent_goals = new HashSet<string>();
            foreach (var element in parent_elements)
            {
                if (element is DefLocalElement le)
                {
                    parent_goals.Add(le.Name);
                }
                else if (element is DefGlobalElement ge)
                {
                    parent_goals.Add(ge.Name);
                }
                else if (element is ConstElement ce)
                {
                    parent_goals.Add(ce.Name);
                }
            }

            var child_goals = new List<string>();
            foreach (var element in child_elements)
            {
                if (element is DefLocalElement le)
                {
                    child_goals.Add(le.Name);
                }
            }

            foreach (var goal in child_goals.Where(name => parent_goals.Contains(name)))
            {
                var suffix = "";
                var next_id = 0;
                var new_goal = goal + suffix;
                while (parent_goals.Contains(new_goal))
                {
                    suffix = "-" + next_id.ToString().Trim();
                    next_id++;
                    new_goal = goal + suffix;
                }

                //Debug.WriteLine("Changed known goal " + goal + " to " + new_goal);

                for (int j = 0; j < child_elements.Count; j++)
                {
                    var code = child_elements[j].Code;

                    code = code.Replace(" " + goal + " ", " " + new_goal + " ");
                    code = code.Replace("(" + goal + " ", "(" + new_goal + " ");
                    code = code.Replace(" " + goal + ")", " " + new_goal + ")");
                    code = code.Replace("(" + goal + ",", "(" + new_goal + ",");
                    code = code.Replace("," + goal + ")", "," + new_goal + ")");
                    if (code.StartsWith(goal + " "))
                    {
                        code = code.Replace(goal + " ", new_goal + " ");
                    }
                    if (code.EndsWith(" " + goal))
                    {
                        code = code.Replace(" " + goal, " " + new_goal);
                    }

                    child_elements[j] = Element.Create(code);
                }
            }

            parent_elements.InsertRange(i, child_elements);
        }
    }
}
