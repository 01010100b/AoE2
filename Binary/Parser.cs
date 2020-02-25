using Binary.Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Binary
{
    class Parser
    {
        public static string ParseFact(string fact)
        {
            fact = fact.Trim();

            if (fact.StartsWith("gl-") || fact.StartsWith("sn-"))
            {
                var pieces = fact.Split(Language.WHITESPACE).Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
                Assert.That(pieces.Count == 3, "invalid fact: " + fact);

                var a = pieces[0];
                var op = pieces[1];
                var b = pieces[2];

                Assert.That(Language.COMPARISONS.Contains(op), "invalid comparison: " + fact);

                var type = "c:";
                if (b.StartsWith("gl-"))
                {
                    type = "g:";
                }
                else if (b.StartsWith("sn-"))
                {
                    type = "s:";
                }

                if (a.StartsWith("gl-"))
                {
                    return $"up-compare-goal {a} {type}{op} {b}";
                }
                else
                {
                    return $"up-compare-sn {a} {type}{op} {b}";
                }
            }
            else
            {
                return fact;
            }
        }

        public Dictionary<string, List<Element>> Parse(Dictionary<string, string[]> files)
        {
            var results = new Dictionary<string, List<Element>>();

            foreach (var kvp in files)
            {
                var elements = new List<Element>();
                try
                {
                    var lines = kvp.Value.ToList();
                    ParseForeach(lines);
                    elements = ParseRaw(lines.ToArray());
                    ParseStatements(elements);
                }
                catch (Exception e)
                {
                    throw new Exception("In file " + kvp.Key + " " + e.Message, e);
                }
                
                results.Add(kvp.Key, elements);

                Trace.WriteLine("Parsed: " + kvp.Key);
            }

            return results;
        }

        private List<Element> ParseRaw(string[] lines)
        {

            var elements = new List<Element>();
            var current_element = new StringBuilder();

            var depth = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    var line = lines[i];
                    var code = line.Trim();

                    Assert.That(!code.StartsWith("(load-random"), "load-random not supported");

                    if (line.Contains(";"))
                    {
                        code = line.Substring(0, line.IndexOf(";")).Trim();
                    }

                    if (!(code.Contains("(") || code.Contains(")"))) // no elements being opened or closed here
                    {
                        if (depth == 0)
                        {
                            elements.Add(Element.Create(line));
                        }
                        else
                        {
                            current_element.AppendLine(line);
                        }
                    }
                    else
                    {
                        var delta_depth = 0;
                        var in_string_literal = false;
                        foreach (var ch in code)
                        {
                            if (ch == '"')
                            {
                                in_string_literal = !in_string_literal;
                            }
                            else if (!in_string_literal)
                            {
                                if (ch == '(')
                                {
                                    delta_depth++;
                                }
                                else if (ch == ')')
                                {
                                    delta_depth--;
                                }
                            }
                        }
                        depth += delta_depth;

                        Assert.That(depth >= 0, "depth < 0: " + code);

                        if (depth == 0)
                        {
                            current_element.Append(line);

                            elements.Add(Element.Create(current_element.ToString()));
                            current_element.Clear();
                        }
                        else
                        {
                            current_element.AppendLine(line);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("at line " + (i + 1) + ": " + e.Message, e);
                }
            }

            return elements;
        }

        private void ParseForeach(List<string> lines)
        {
            var parsed = true;
            while (parsed)
            {
                parsed = false;

                var start_foreach = -1;
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i].Trim();

                    if (line.StartsWith("#foreach"))
                    {
                        start_foreach = i;

                        Assert.That(!line.Contains(";"), "no comments on #foreach lines");
                    }
                    else if (line.StartsWith("#end-foreach"))
                    {
                        var end_foreach = i;

                        // transform new_lines
                        var new_lines = new List<string>();

                        var range = lines[start_foreach].Split(' ').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
                        range.RemoveAt(0);

                        var wildcard = range[0].Split(':');
                        range.RemoveAt(0);

                        Assert.That(range[0].Equals("in"), "invalid #foreach syntax: " + lines[start_foreach]);
                        Assert.That(range.Count(s => s.Contains("{") || s.Contains("}")) == 0, "Curly braces not allowed in foreach: " + lines[start_foreach]);
                        range.RemoveAt(0);

                        foreach (var val in range.Select(v => v.Split(':')))
                        {
                            for (int j = start_foreach + 1; j < end_foreach; j++)
                            {
                                var code = lines[j];

                                for (int k = 0; k < wildcard.Length; k++)
                                {
                                    code = code.Replace(wildcard[k], val[k]);
                                }

                                new_lines.Add(code);
                            }
                        }

                        lines.RemoveRange(start_foreach, end_foreach - start_foreach + 1);
                        lines.InsertRange(start_foreach, new_lines);

                        parsed = true;
                        break;
                    }
                }
            }
        }

        private void ParseStatements(List<Element> elements)
        {
            // first pass, handle special statements, do syntax checking

            for (int i = 0; i < elements.Count; i++)
            {
                var code = elements[i].ActualCode;

                if (code.StartsWith("var"))
                {
                    elements[i] = Element.Create("#deflocal" + elements[i].Code.Substring(3));
                }

                if (elements[i] is StatementElement)
                {
                    if (code.Contains("(") && code.Contains(")")) // function call
                    {
                        if ((code.StartsWith("gl-") || code.StartsWith("sn-")) && code.Contains(" = ")) // assignment
                        {
                            elements[i] = Element.Create("#call " + code.Replace("=", "").Replace("(", " ").Replace(",", " ").Replace(")", ""));
                        }
                        else if (!code.Contains("=")) // bare function call
                        {
                            elements[i] = Element.Create("#call glx-return-value " + code.Replace("(", " ").Replace(",", " ").Replace(")", ""));
                        }
                    }
                }

                if (code.StartsWith("var ") || code.StartsWith("#deflocal") || code.StartsWith("#defglobal") || code.StartsWith("#defexclusive") || code.StartsWith("#defstrategic"))
                {
                    var pieces = elements[i].Pieces;
                    
                    Assert.That(pieces.Count == 2 || pieces.Count == 4, $"invalid syntax: {elements[i].Code}");

                    var name = pieces[1];

                    if (code.StartsWith("#defstrategic"))
                    {
                        Assert.That(name.StartsWith("sn-"), $"invalid name (use sn-*): {code}");
                    }
                    else
                    {
                        Assert.That(name.StartsWith("gl-"), $"invalid name (use gl-*): {code}");
                    }
                }
            }

            ParseInitializers(elements);

            // second pass, handle math/assignment statements

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] is StatementElement element)
                {
                    var code = element.ActualCode;

                    if (code.StartsWith("gl-") || code.StartsWith("sn-"))
                    {
                        var pieces = element.Pieces;

                        Assert.That(pieces.Count == 3, $"invalid statement: {elements[i].Code}");

                        var res = pieces[0];
                        var action = res.StartsWith("gl-") ? "up-modify-goal" : "up-modify-sn";
                        var op = pieces[1];
                        var exp = pieces[2];

                        Assert.That(Language.ASSIGNMENTS.Contains(op), $"invalid operator: {elements[i].Code}");

                        var type = "c:";
                        if (exp.StartsWith("gl-"))
                        {
                            type = "g:";
                        }
                        else if (exp.StartsWith("sn-"))
                        {
                            type = "s:";
                        }

                        elements.RemoveAt(i);
                        if (op.Equals("=")) // assignment
                        {
                            elements.Insert(i, Element.Create(action + " " + res + " " + type + "= " + exp));
                        }
                        else // math
                        {
                            elements.Insert(i, Element.Create(action + " " + res + " " + type + op.Replace("=", "") + " " + exp));
                        }
                    }
                }
            }

            // third pass, actually put statement blocks into rules

            var index = -1;
            var statements = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                var combine = false;
                if (elements[i] is StatementElement)
                {
                    if (index == -1)
                    {
                        index = i;
                    }

                    statements++;

                    if (statements >= 15)
                    {
                        combine = true;
                    }
                    
                }
                else if (!(elements[i] is CommentElement || index == -1))
                {
                    i--;
                    combine = true;
                }
                
                if (i == elements.Count - 1 && index != -1 && statements > 0)
                {
                    combine = true;
                }

                if (combine)
                {
                    while (!(elements[i] is StatementElement))
                    {
                        i--;
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine("(defrule");
                    sb.AppendLine("\t(true)");
                    sb.AppendLine("=>");

                    for (int j = index; j <= i; j++)
                    {
                        var element = elements[j];

                        if (element is StatementElement)
                        {
                            sb.AppendLine($"\t({element.ActualCode})");
                        }
                        else if (element is CommentElement)
                        {
                            if (element.Code.Trim().Length >= 0)
                            {
                                sb.AppendLine($"\t{element.Code.Trim()}");
                            }
                        }
                    }

                    sb.Append(")");

                    var length = i - index + 1;
                    elements.RemoveRange(index, length);
                    //elements.Insert(index, Element.Create(""));
                    elements.Insert(index, Element.Create(sb.ToString()));

                    i -= length - 1;

                    index = -1;
                    statements = 0;
                }
            }
        }

        private void ParseInitializers(List<Element> elements)
        {
            var initializers = new List<KeyValuePair<string, string>>();
            var local_initializers = new List<KeyValuePair<string, string>>();

            for (int i = 0; i < elements.Count; i++)
            {
                var code = elements[i].ActualCode;

                if (code.StartsWith("#deflocal") || code.StartsWith("#defglobal") || code.StartsWith("#defexclusive") || code.StartsWith("#defstrategic"))
                {
                    var pieces = elements[i].Pieces;
                    var name = pieces[1];

                    if (pieces.Count == 4) // initialization
                    {
                        Assert.That(pieces[2].Equals("="), $"invalid initialization syntax: {elements[i].Code}");

                        elements[i] = Element.Create($"{pieces[0]} {name}");

                        var initial_value = pieces[3];
                        if (code.StartsWith("#deflocal"))
                        {
                            if (initial_value.StartsWith("gl-"))
                            {
                                elements.Insert(i + 1, Element.Create($"(defrule\n\t(true)\n=>\n\t(up-modify-goal {name} g:= {initial_value})\n)"));
                            }
                            else if (initial_value.StartsWith("sn-"))
                            {
                                elements.Insert(i + 1, Element.Create($"(defrule\n\t(true)\n=>\n\t(up-modify-goal {name} s:= {initial_value})\n)"));
                            }
                            else
                            {
                                local_initializers.Add(new KeyValuePair<string, string>(name, initial_value));
                            }
                        }
                        else
                        {
                            Assert.That(!(code.StartsWith("gl-") || code.StartsWith("sn-")),
                                "Can only initialize globals to constants: " + elements[i].Code);

                            initializers.Add(new KeyValuePair<string, string>(name, initial_value));
                        }
                    }
                }

                if (initializers.Count > 0)
                {
                    var initialize = false;

                    if (code.Length > 0 && !(code.StartsWith("(defconst") || code.StartsWith("#def")))
                    {
                        initialize = true;
                    }
                    else if (initializers.Count >= 14)
                    {
                        initialize = true;
                        i++;
                    }
                    else if (i == elements.Count - 1)
                    {
                        initialize = true;
                        i++;
                    }

                    if (initialize)
                    {
                        var rule = new StringBuilder();

                        rule.AppendLine("(defrule");
                        rule.AppendLine("\t(true)");
                        rule.AppendLine("=>");

                        foreach (var init in initializers)
                        {
                            if (init.Key.StartsWith("gl-"))
                            {
                                rule.AppendLine($"\t(set-goal {init.Key} {init.Value})");
                            }
                            else
                            {
                                rule.AppendLine($"\t(set-strategic-number {init.Key} {init.Value})");
                            }
                        }

                        rule.AppendLine("\t(disable-self)");
                        rule.AppendLine(")");

                        elements.Insert(i, Element.Create(""));
                        elements.Insert(i, Element.Create(rule.ToString()));
                        elements.Insert(i, Element.Create(""));

                        initializers.Clear();
                    }
                }

                if (local_initializers.Count > 0)
                {
                    var initialize = false;

                    if (code.Length > 0 && !(code.StartsWith("(defconst") || code.StartsWith("#def")))
                    {
                        initialize = true;
                    }
                    else if (local_initializers.Count >= 15)
                    {
                        initialize = true;
                        i++;
                    }
                    else if (i == elements.Count - 1)
                    {
                        initialize = true;
                        i++;
                    }

                    if (initialize)
                    {
                        var rule = new StringBuilder();

                        rule.AppendLine("(defrule");
                        rule.AppendLine("\t(true)");
                        rule.AppendLine("=>");

                        foreach (var init in local_initializers)
                        {
                            if (init.Key.StartsWith("gl-"))
                            {
                                rule.AppendLine($"\t(set-goal {init.Key} {init.Value})");
                            }
                            else
                            {
                                rule.AppendLine($"\t(set-strategic-number {init.Key} {init.Value})");
                            }
                        }

                        rule.AppendLine(")");

                        elements.Insert(i, Element.Create(""));
                        elements.Insert(i, Element.Create(rule.ToString()));
                        elements.Insert(i, Element.Create(""));

                        local_initializers.Clear();
                    }
                }
            }
        }
    }
}
