using Binary.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary
{
    abstract class Element
    {
        public static Element Create(string code)
        {
            var trimmed_code = code.Trim();

            if (trimmed_code.Length == 0 || trimmed_code.StartsWith(";")) // comments
            {
                return new CommentElement(code);
            }
            else if (trimmed_code.StartsWith("(")) // standard constructs
            {
                if (trimmed_code.StartsWith("(defconst"))
                {
                    return new ConstElement(code);
                }
                else if (trimmed_code.StartsWith("(defrule"))
                {
                    return new RuleElement(code);
                }
                else if (trimmed_code.StartsWith("(load"))
                {
                    if (trimmed_code.StartsWith("(load-random"))
                    {
                        return new LoadRandomElement(code);
                    }
                    else
                    {
                        return new LoadElement(code);
                    }
                }
            }
            else if (trimmed_code.StartsWith("#")) // preprocessor constructs
            {
                if (trimmed_code.StartsWith("#load-if"))
                {
                    return new LoadIfDefinedElement(code);
                }
                else if (trimmed_code.StartsWith("#deflocal"))
                {
                    return new DefLocalElement(code);
                }
                else if (trimmed_code.StartsWith("#defglobal"))
                {
                    return new DefGlobalElement(code);
                }
                else if (trimmed_code.StartsWith("#defstrategic"))
                {
                    return new DefStrategicElement(code);
                }
                else if (Language.PREPROCESSOR_DIRECTIVES.Count(p => trimmed_code.StartsWith(p)) > 0)
                {
                    return new PreprocessorElement(code);
                }
            }
            else // statements
            {
                return new StatementElement(code);
            }

            throw new Exception("Element not recognized: " + code);
        }

        public abstract int RuleLength { get; }
        public string Code { get; protected set; }
        public string ActualCode => GetActualCode(); // remove comments
        public List<string> Pieces => GetPieces();

        protected Element (string code)
        {
            Code = code.Trim();
        }

        public void Compactify()
        {
            Code = ActualCode;
            if (Code.Length > 255)
            {
                var lines = Code.Split('\n', '\r');
                var new_lines = new List<string>();

                foreach (var line in lines)
                {
                    if (line.Length < 256)
                    {
                        new_lines.Add(line);
                    }
                    else
                    {
                        var pieces = line.Split('(');
                        var new_line = new StringBuilder();

                        foreach (var piece in pieces)
                        {
                            new_line.Append("(" + piece + " ");

                            if (new_line.Length > 100)
                            {
                                new_lines.Add(new_line.ToString());
                                new_line.Clear();
                            }
                        }

                        if (new_line.Length > 0)
                        {
                            new_lines.Add(new_line.ToString());
                        }
                    }
                }

                var code = new StringBuilder();
                foreach (var line in new_lines)
                {
                    code.AppendLine(line);
                }

                Code = code.ToString();
            }
        }

        private string GetActualCode()
        {
            var lines = Code.Split('\n', '\r');
            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                var res = line.Trim();
                if (line.Contains(";"))
                {
                    res = line.Substring(0, line.IndexOf(";")).Trim();
                }

                if (res.Length > 0)
                {
                    sb.AppendLine(res);
                }
            }

            return sb.ToString().Trim();
        }

        private List<string> GetPieces()
        {
            return ActualCode.Split(Language.WHITESPACE).Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
        }
    }
}
