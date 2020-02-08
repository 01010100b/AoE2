using Binary.Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binary
{
    class Compiler
    {
        private static string ParseCondition(string condition)
        {
            condition = condition.Trim();

            if (!condition.StartsWith("("))
            {
                condition = "(" + condition + ")"; // h
            }

            var sb = new StringBuilder();

            foreach (var ch in condition.ToList())
            {
                if (ch == '(')
                {
                    sb.Clear();
                }
                else if (ch == ')')
                {
                    var fact = sb.ToString();

                    if (fact.Trim().Length > 0)
                    {
                        var new_fact = Parser.ParseFact(fact);
                        condition = condition.Replace(fact, new_fact);
                    }
                    
                    sb.Clear();
                }
                else
                {
                    sb.Append(ch);
                }
            }

            var pieces = new List<string>();

            var depth = 0;
            var current_piece = new StringBuilder();
            foreach (var ch in condition)
            {
                if (ch == '(')
                {
                    if (depth == 0 && current_piece.ToString().Trim().Length > 0)
                    {
                        pieces.Add(current_piece.ToString().Trim());
                        current_piece.Clear();
                    }

                    depth++;

                    current_piece.Append(ch);
                }
                else if (ch == ')')
                {
                    current_piece.Append(ch);
                    depth--;

                    if (depth == 0)
                    {
                        pieces.Add(current_piece.ToString().Trim());
                        current_piece.Clear();
                    }
                }
                else
                {
                    current_piece.Append(ch);
                }
            }

            while (pieces.Count > 1)
            {
                var piece = $"(and\n {pieces[0]}\n {pieces[1]})";
                pieces[0] = piece;
                pieces.RemoveAt(1);
            }

            return pieces[0];
        }

        private class Block
        {
            public bool Flattened => Elements.Where(e => e is Block).Count() == 0;
            private readonly List<object> Elements = new List<object>();

            public void Add(Element element)
            {
                Elements.Add(element);
            }

            public void Add(Block block)
            {
                Elements.Add(block);
            }

            public List<Element> GetElements()
            {
                return Elements.Where(e => e is Element).Select(e => (Element)e).ToList();
            }

            public List<Block> GetSubBlocks()
            {
                return Elements.Where(e => e is Block).Select(e => (Block)e).ToList();
            }

            public void SetElements(IEnumerable<Element> elements)
            {
                Elements.Clear();
                Elements.AddRange(elements);
            }

            public void Flatten()
            {
                if (Flattened)
                {
                    return;
                }

                for (int i = 0; i < Elements.Count; i++)
                {
                    var element = Elements[i];

                    if (element is Block block)
                    {
                        block.Flatten();

                        Elements.RemoveAt(i);
                        Elements.InsertRange(i, block.GetElements());
                    }
                }
            }
        }

        public int FreeStrategicNumbers { get; private set; } = 0;

        public void Compile(List<Element> elements) 
        {
            // preprocessor

            Debug.WriteLine("Start repeating timers");
            RepeatingTimers(elements);
            Debug.WriteLine("Done repeating timers");

            Debug.WriteLine("Start preprocessor");
            TransformPreprocessor(elements);
            Debug.WriteLine("Done preprocessor");

            // goal and sn assignment

            Debug.WriteLine("Start strategic numbers");
            StrategicNumbers(elements);
            Debug.WriteLine("Done strategic numbers");
            
            Debug.WriteLine("Start global variables");
            GlobalVariables(elements);
            Debug.WriteLine("Done global variables");

            Debug.WriteLine("Start local variables");
            LocalVariables(elements);
            Debug.WriteLine("Done local variables");

            Debug.WriteLine("Start constant shadowing");
            ConstantShadowing(elements);
            Debug.WriteLine("Done constant shadowing");

            foreach (var element in elements)
            {
                element.Compactify();
            }
        }

        private void StrategicNumbers(List<Element> elements)
        {
            const int FIRST = 296;
            const int LAST = 511;

            var available = new HashSet<int>();
            for (int i = FIRST; i <= LAST; i++)
            {
                available.Add(i);
            }
            foreach (var element in elements.Where(e => e is ConstElement).Select(e => (ConstElement)e))
            {
                if (element.Name.StartsWith("sn-"))
                {
                    if (int.TryParse(element.Value, out int value))
                    {
                        available.Remove(value);
                    }
                }
            }

            var queue = new Queue<int>();
            foreach (var i in available)
            {
                queue.Enqueue(i);
            }

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];

                if (element is DefStrategicElement se)
                {
                    Assert.That(queue.Count > 0, "Not enough free strategic numbers");

                    var number = queue.Dequeue();

                    elements[i] = Element.Create($"(defconst {se.Name} {number})");
                }
            }

            FreeStrategicNumbers = queue.Count;
        }

        private void GlobalVariables(List<Element> elements)
        {
            var available = new HashSet<int>();
            for (int i = 1; i <= Program.MAX_GOALS; i++)
            {
                available.Add(i);
            }
            foreach (var element in elements.Where(e => e is ConstElement).Select(e => (ConstElement)e))
            {
                if (element.Name.StartsWith("gl-") || element.Name.StartsWith("glx-"))
                {
                    if (int.TryParse(element.Value, out int value))
                    {
                        available.Remove(value);
                    }
                }
            }

            var queue = new Queue<string>();
            foreach (var i in available)
            {
                queue.Enqueue(i.ToString().Trim());
            }

            var globals = new Dictionary<string, string>();
            foreach (var element in elements)
            {
                if (element is DefGlobalElement ge)
                {
                    if (!globals.ContainsKey(ge.Name))
                    {
                        globals.Add(ge.Name, queue.Dequeue());
                    }
                }
            }

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];

                if (element is DefGlobalElement ge)
                {
                    elements[i] = Element.Create($"(defconst {ge.Name} {globals[ge.Name]})");
                }
            }
        }

        private void LocalVariables(List<Element> elements)
        {
            // #deflocal gl-whatever
            // glx-temporary-1, glx-temporary-2, ...

            var deflocals = new List<DefLocalElement>();

            Debug.WriteLine("Getting local variable indices");

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];

                if (element is DefLocalElement local)
                {
                    local.Index = i;
                    local.FreeIndex = elements.Count;
                    deflocals.Add(local);
                }
            }

            Debug.WriteLine("Checking multiple definitions");

            for (int i = 0; i < deflocals.Count; i++)
            {
                var local = deflocals[i];

                //Debug.WriteLine($"Checking deflocal {local.Name} {i + 1}/{deflocals.Count}");

                var next_definition = elements.Count;
                for (int j = i + 1; j < deflocals.Count; j++)
                {
                    if (deflocals[j].Name.Equals(local.Name))
                    {
                        next_definition = deflocals[j].Index;
                        break;
                    }
                }

                if (next_definition < elements.Count)
                {
                    local.FreeIndex = next_definition;

                    var name = local.Name;
                    var new_name = name + "-" + local.Index.ToString().Trim();
                    
                    Debug.WriteLine("Renaming local var " + name + " to " + new_name);
                    local.SetName(new_name);

                    for (int j = local.Index + 1; j < local.FreeIndex; j++)
                    {
                        var code = elements[j].Code;

                        code = code.Replace(" " + name + " ", " " + new_name + " ");
                        code = code.Replace("(" + name + " ", "(" + new_name + " ");
                        code = code.Replace(" " + name + ")", " " + new_name + ")");
                        if (code.StartsWith(name + " "))
                        {
                            code = code.Replace(name + " ", new_name + " ");
                        }
                        if (code.EndsWith(" " + name))
                        {
                            code = code.Replace(" " + name, " " + new_name);
                        }

                        elements[j] = Element.Create(code);
                    }
                }
            }
            
            Debug.WriteLine("Getting accurate FreeIndices");

            //foreach (var local in deflocals.Where(l => l.FreeIndex == elements.Count))
            Parallel.ForEach(deflocals.Where(l => l.FreeIndex == elements.Count), local =>
            {
                var test1 = " " + local.Name + " ";
                var test2 = " " + local.Name + ")";
                var test3 = " " + local.Name;

                local.FreeIndex = local.Index + 1;
                for (int i = local.Index + 1; i < elements.Count; i++)
                {
                    var code = elements[i].Code;
                    if (code.Contains(local.Name))
                    {
                        code = elements[i].ActualCode;
                        if (code.Contains(test1) || code.Contains(test2) || code.EndsWith(test3))
                        {
                            local.FreeIndex = i + 1;
                        }
                    }

                }
            });

            Debug.WriteLine("Starting assignment");

            var in_use = new List<DefLocalElement>();
            var max_in_use = 0;

            for (int l = 0; l < deflocals.Count; l++)
            {
                var local = deflocals[l];

                Assert.That(in_use.Count <= 64, "More than 64 concurrently used local vars");

                var handled = false;
                for (int i = 0; i < in_use.Count; i++)
                {
                    var element = in_use[i];

                    var free = true;
                    if (local.Index < element.FreeIndex)
                    {
                        free = false;
                    }
                    
                    if (free)
                    {
                        elements[local.Index] = Element.Create($"(defconst {local.Name} glx-temporary-{i + 1})");
                        if (local.Initial != null)
                        {
                            var rule = $"(defrule\n\t(true)\n=>\n\t(set-goal {local.Name} {local.Initial})\n)";
                            elements.Insert(local.Index + 1, Element.Create(rule));
                            
                            local.FreeIndex++;
                            for (int j = l + 1; j < deflocals.Count; j++)
                            {
                                deflocals[j].Index++;
                                deflocals[j].FreeIndex++;
                            }
                        }

                        in_use[i] = local;
                        handled = true;
                    }

                    if (handled)
                    {
                        break;
                    }
                }

                if (!handled)
                {
                    elements[local.Index] = Element.Create($"(defconst {local.Name} glx-temporary-{in_use.Count + 1})");
                    in_use.Add(local);
                    max_in_use = Math.Max(max_in_use, in_use.Count);

                    if (local.Initial != null)
                    {
                        var rule = $"(defrule\n\t(true)\n=>\n\t(set-goal {local.Name} {local.Initial})\n)";
                        elements.Insert(local.Index + 1, Element.Create(rule));

                        local.FreeIndex++;
                        for (int j = l + 1; j < deflocals.Count; j++)
                        {
                            deflocals[j].Index++;
                            deflocals[j].FreeIndex++;
                        }
                    }
                }

                Debug.WriteLine("Assigned: " + local.Name);
            }

            Debug.WriteLine("Used " + max_in_use + " temporary ids");
        }

        private void ConstantShadowing(List<Element> elements)
        {
            var defined = new Dictionary<string, string>();
            var undefined = new Dictionary<string, string>();

            foreach (var element in elements.Where(e => e is ConstElement).Select(e => (ConstElement)e))
            {
                if (!element.Value.Contains("\""))
                {
                    if (defined.ContainsKey(element.Name))
                    {
                        Assert.That(element.Value.Trim().Equals(defined[element.Name].Trim()), "Already defined: " + element.ActualCode);
                    }
                    else if (undefined.ContainsKey(element.Name))
                    {
                        Assert.That(element.Value.Trim().Equals(undefined[element.Name].Trim()), "Already undefined: " + element.ActualCode);
                    }
                    
                    if (int.TryParse(element.Value, out int res))
                    {
                        defined[element.Name] = element.Value;
                    }
                    else
                    {
                        undefined[element.Name] = element.Value;
                    }
                }
            }

            foreach (var kvp in undefined.ToList())
            {
                if (!(defined.ContainsKey(kvp.Value) || undefined.ContainsKey(kvp.Value)))
                {
                    undefined.Remove(kvp.Key);
                    Debug.WriteLine("Ignored: (defconst " + kvp.Key + " " + kvp.Value + ")");
                }
            }

            while (undefined.Count > 0)
            {
                var found = false;
                foreach (var kvp in undefined.ToList())
                {
                    if (defined.ContainsKey(kvp.Value))
                    {
                        var value = short.Parse(defined[kvp.Value]).ToString().Trim();

                        undefined.Remove(kvp.Key);
                        defined.Add(kvp.Key, value);

                        found = true;
                    }
                }

                Assert.That(found, "defconst not found: " + undefined.FirstOrDefault().Key + " -> " + undefined.FirstOrDefault().Value);
            }

            foreach (var element in elements.Where(e => e is ConstElement).Select(e => (ConstElement)e))
            {
                if (defined.ContainsKey(element.Name))
                {
                    element.SetValue(defined[element.Name]);
                }
            }
        }

        private void RepeatingTimers(List<Element> elements)
        {
            var timers = elements.Where(e => e.ActualCode.StartsWith("#repeat-timer ")).ToList();
            elements.RemoveAll(e => timers.Contains(e));

            foreach (var timer in timers)
            {
                // #repeat-timer name value period
                var pieces = timer.Pieces;
                var name = pieces[1].Trim();
                var value = short.Parse(pieces[2]);
                var period = short.Parse(pieces[3]);

                var timer_def = $"(defconst {name} {value}) ; repeating every {period} seconds";
                var timer_setup = $"(defrule\n\t(true)\n=>\n\t(enable-timer {name} {period})\n\t(disable-self)\n)";
                var timer_reenable = $"(defrule\n\t(timer-triggered {name})\n=>\n\t(disable-timer {name})\n\t(enable-timer {name} {period})\n)";

                elements.Insert(0, Element.Create(""));
                elements.Insert(0, Element.Create(timer_setup));
                elements.Insert(0, Element.Create(timer_def));
                elements.Add(Element.Create(""));
                elements.Add(Element.Create(timer_reenable));
            }
        }

        private void TransformPreprocessor(List<Element> elements)
        {
            var blocks = new Stack<Block>();
            var current_block = new Block();

            foreach (var element in elements)
            {
                var start_block = false;
                var end_block = false;

                var code = element.ActualCode;
                if (code.StartsWith("#if") || code.StartsWith("#load-if") || code.StartsWith("#for ") || code.StartsWith("#while ") || code.StartsWith("#foreach "))
                {
                    start_block = true;
                }
                else if (code.StartsWith("#end-if") || code.StartsWith("#end-for") || code.StartsWith("#end-while") || code.StartsWith("#end-foreach"))
                {
                    end_block = true;
                }

                if (start_block)
                {
                    var new_current_block = new Block();
                    current_block.Add(new_current_block);

                    blocks.Push(current_block);
                    current_block = new_current_block;

                    current_block.Add(element);
                }
                else if (end_block)
                {
                    current_block.Add(element);
                    current_block = blocks.Pop();
                }
                else
                {
                    current_block.Add(element);
                }
            }

            Debug.Assert(blocks.Count == 0, "Preprocessor block not closed"); // stack should be empty again

            TransformBlock(current_block);

            elements.Clear();
            elements.AddRange(current_block.GetElements());
        }

        private void TransformBlock(Block block)
        {
            foreach (var sub_block in block.GetSubBlocks())
            {
                TransformBlock(sub_block);
            }

            block.Flatten();

            var elements = block.GetElements().ToList();
            var code = elements[0].ActualCode;

            if (elements[0] is LoadIfDefinedElement element)
            {
                if (!element.Transformed)
                {
                    TransformLoadIfDefined(elements);
                }
            }
            else if (code.StartsWith("#if ") || code.StartsWith("#if-timer "))
            {
                TransformIf(elements);
            }
            else if (code.StartsWith("#for "))
            {
                TransformFor(elements);
            }
            else if (code.StartsWith("#while "))
            {
                TransformWhile(elements);
            }
            else if (code.StartsWith("#foreach "))
            {
                throw new Exception("#foreach not yet handled by parser: " + elements[0].Code);
            }

            block.SetElements(elements);
        }

        private void TransformLoadIfDefined(List<Element> elements)
        {
            // TODO fix this again, allow both defconsts and rules, put defconsts in load-if-defined at the top
            // #load-if-defined CONSTANT
            // #load-if-not-defined CONSTANT

            Assert.That(elements.Count(e => e is RuleElement) == 0 || elements.Count(e => e is ConstElement) == 0, 
                "#load-if-defined with both defconsts and defrules: " + elements[0].Code);

            if (elements.Count(e => e is RuleElement) == 0) // no rules
            {
                var element = (LoadIfDefinedElement)elements[0];
                element.SetRuleLength(0);
                element.Transformed = true;

                return;
            }
            else // no consts
            {
                var constant = elements[0].Pieces[1];

                var sb = new StringBuilder();
                sb.AppendLine("#load-if-defined " + constant);
                sb.AppendLine("");
                sb.AppendLine("(defrule");
                sb.AppendLine("\t(true)");
                sb.AppendLine("=>");
                sb.AppendLine("\t(set-goal glx-is-defined 1)");
                sb.AppendLine(")");
                sb.AppendLine("");
                sb.AppendLine("#else");
                sb.AppendLine("");
                sb.AppendLine("(defrule");
                sb.AppendLine("\t(true)");
                sb.AppendLine("=>");
                sb.AppendLine("\t(set-goal glx-is-defined 0)");
                sb.AppendLine(")");
                sb.AppendLine("");
                sb.AppendLine("#end-if");

                var prefix = (LoadIfDefinedElement)Element.Create(sb.ToString());
                prefix.SetRuleLength(1);
                prefix.Transformed = true;

                if (elements[0].ActualCode.StartsWith("#load-if-defined"))
                {
                    elements.RemoveAt(0);
                    elements.Insert(0, Element.Create("#if goal glx-is-defined 1"));
                }
                else
                {
                    elements.RemoveAt(0);
                    elements.Insert(0, Element.Create("#if goal glx-is-defined 0"));
                }

                Debug.Assert(elements.Count != 1);

                TransformIf(elements);

                elements.Insert(0, Element.Create(""));
                elements.Insert(0, prefix);
                elements.Insert(0, Element.Create("#deflocal glx-is-defined"));
            }
        }

        private void TransformIf(List<Element> elements)
        {
            // #if condition // #if-timer timer-var
            // #else (optional)
            // #end-if

            Assert.That(!elements[0].ActualCode.StartsWith("#if-timer"), 
                "#if-timer deprecated, use #if timer-triggered instead: " + elements[0].Code);

            var condition = ParseCondition(elements[0].ActualCode.Replace("#if", "").Trim());

            var true_elements = new List<Element>();
            var false_elements = new List<Element>();
            var current_elements = true_elements;

            // remove #if and #end-if
            elements.RemoveAt(0);
            elements.RemoveAt(elements.Count - 1);

            foreach (var element in elements)
            {
                if (element.ActualCode.StartsWith("#else"))
                {
                    current_elements = false_elements;
                }
                else
                {
                    current_elements.Add(element);
                }
            }

            var new_elements = new List<Element>();

            if (false_elements.Count == 0) // no #else
            {
                var jump = true_elements.Sum(e => e.RuleLength);
                var test = $"(defrule\n\t(not {condition})\n=>\n\t(up-jump-dynamic c: {jump})\n)";

                new_elements.Add(Element.Create($";if {condition.Replace("\n", " ").Replace("\r", " ")}"));
                new_elements.Add(Element.Create(""));
                new_elements.Add(Element.Create(test));
                new_elements.AddRange(true_elements);
                new_elements.Add(Element.Create(";end-if"));
            }
            else
            {
                var jump1 = true_elements.Sum(e => e.RuleLength) + 1;
                var jump2 = false_elements.Sum(e => e.RuleLength);

                var test = $"(defrule\n\t(not {condition})\n=>\n\t(up-jump-dynamic c: {jump1})\n)";
                var jump_else = $"(defrule\n\t(true)\n=>\n\t(up-jump-dynamic c: {jump2})\n)";

                new_elements.Add(Element.Create($";if {condition.Replace("\n", " ").Replace("\r", " ")}"));
                new_elements.Add(Element.Create(""));
                new_elements.Add(Element.Create(test));
                new_elements.AddRange(true_elements);
                new_elements.Add(Element.Create(jump_else));
                new_elements.Add(Element.Create(""));
                new_elements.Add(Element.Create(";else"));
                new_elements.AddRange(false_elements);
                new_elements.Add(Element.Create(";end-if"));
            }

            elements.Clear();
            elements.AddRange(new_elements);
        }

        private void TransformFor(List<Element> elements)
        {
            // #for min max loop-var
            // #end-for

            var pieces = elements[0].Pieces;
            var min = pieces[1];
            var max = pieces[2];
            var loop_var = pieces[3];

            var rules = elements.Sum(e => e.RuleLength);
            var jump_forward = rules + 1;
            var jump_backward = -(rules + 2);

            var init = $"(defrule\n\t(true)\n=>\n\t(up-modify-goal {loop_var} c:= {min})\n)";
            if (min.StartsWith("gl-"))
            {
                init = $"(defrule\n\t(true)\n=>\n\t(up-modify-goal {loop_var} g:= {min})\n)";
            }
            else if (min.StartsWith("sn-"))
            {
                init = $"(defrule\n\t(true)\n=>\n\t(up-modify-goal {loop_var} s:= {min})\n)";
            }

            var condition = $"(defrule\n\t(up-compare-goal {loop_var} c:>= {max})\n=>\n\t(up-jump-dynamic c: {jump_forward})\n)";
            if (max.StartsWith("gl-"))
            {
                condition = $"(defrule\n\t(up-compare-goal {loop_var} g:>= {max})\n=>\n\t(up-jump-dynamic c: {jump_forward})\n)";
            }
            else if (max.StartsWith("sn-"))
            {
                condition = $"(defrule\n\t(up-compare-goal {loop_var} s:>= {max})\n=>\n\t(up-jump-dynamic c: {jump_forward})\n)";
            }

            var increment = $"(defrule\n\t(true)\n=>\n\t(up-modify-goal {loop_var} c:+ 1)\n\t(up-jump-dynamic c: {jump_backward})\n)";

            // remove preprocessor directives
            elements.RemoveAt(0);
            elements.RemoveAt(elements.Count - 1);

            var new_elements = new List<Element>();
            new_elements.Add(Element.Create($";for {loop_var} from {min} to {max}"));
            new_elements.Add(Element.Create($"#deflocal {loop_var}"));
            new_elements.Add(Element.Create(""));
            new_elements.Add(Element.Create(init));
            new_elements.Add(Element.Create(""));
            new_elements.Add(Element.Create(condition));
            new_elements.AddRange(elements);
            new_elements.Add(Element.Create(increment));
            new_elements.Add(Element.Create(""));
            new_elements.Add(Element.Create(";end for"));

            elements.Clear();
            elements.AddRange(new_elements);
        }

        private void TransformWhile(List<Element> elements)
        {
            // #while condition
            // #end-while

            var condition = ParseCondition(elements[0].ActualCode.Replace("#while", "").Trim());

            var rules = elements.Sum(e => e.RuleLength);
            var jump_forward = rules + 1;
            var jump_backward = -(rules + 2);

            var test = $"(defrule\n\t(not {condition})\n=>\n\t(up-jump-dynamic c: {jump_forward})\n)";
            var backwards = $"(defrule\n\t(true)\n=>\n\t(up-jump-dynamic c: {jump_backward})\n)";

            // remove preprocessor directives
            elements.RemoveAt(0);
            elements.RemoveAt(elements.Count - 1);

            var new_elements = new List<Element>();
            new_elements.Add(Element.Create($";while {condition.Replace("\n", " ").Replace("\r", " ")}"));
            new_elements.Add(Element.Create(""));
            new_elements.Add(Element.Create(test));
            new_elements.AddRange(elements);
            new_elements.Add(Element.Create(backwards));
            new_elements.Add(Element.Create(""));
            new_elements.Add(Element.Create(";end while"));

            elements.Clear();
            elements.AddRange(new_elements);
        }

        private void TransformForeach(List<Element> elements)
        {
            // #foreach $wildcard-array in string1-array string2-array ...
            // #end-foreach

            var range = elements[0].Pieces;
            range.RemoveAt(0);
            
            var wildcard = range[0].Split(':');
            range.RemoveAt(0);

            Assert.That(range[0].Equals("in"), "invalid #foreach syntax: " + elements[0].Code);
            Assert.That(range.Count(s => s.Contains("{") || s.Contains("}")) == 0, "Curly braces not allowed in foreach: " + elements[0].Code);
            range.RemoveAt(0);

            // remove preprocessor directives and possible empty line at top
            elements.RemoveAt(0);
            elements.RemoveAt(elements.Count - 1);
            if (elements[0].ActualCode.Equals(""))
            {
                elements.RemoveAt(0);
            }

            var new_elements = new List<Element>();
            new_elements.Add(Element.Create(";foreach " + wildcard));
            new_elements.Add(Element.Create(""));

            foreach (var val in range.Select(v => v.Split(':')))
            {
                foreach (var element in elements)
                {
                    var code = element.Code;

                    for (int i = 0; i < wildcard.Length; i++)
                    {
                        code = code.Replace(wildcard[i], val[i]);
                    }
                    
                    new_elements.Add(Element.Create(code));
                }
            }

            new_elements.Add(Element.Create(";end foreach"));

            elements.Clear();
            elements.AddRange(new_elements);
        }
    }
}
