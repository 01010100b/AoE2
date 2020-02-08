using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary
{
    static class Language
    {
        public static readonly char[] WHITESPACE = { ' ', '\t', '\n', '\r' };
        public static readonly string[] COMPARISONS = { "==", "!=", "<", "<=", ">", ">=" };
        public static readonly string[] ASSIGNMENTS = { "=", "+=", "-=", "*=", "/=", "mod=", "min=", "max=", "neg=", "z/=", "%*=", "%/=" };

        public static string[] PREPROCESSOR_DIRECTIVES { get { return STANDARD_PREPROCESSOR_DIRECTIVES.Concat(EXTENDED_PREPROCESSOR_DIRECTIVES).ToArray(); } }
        public static readonly string[] EXTENDED_PREPROCESSOR_DIRECTIVES = { "#defglobal", "#deflocal", "#defexclusive", "#defstrategic", "#repeat-timer", "#if", "#for", "#end-for", "#while", "#end-while", "#foreach", "#end-foreach", "#call", "#function", "#return" };
        public static readonly string[] STANDARD_PREPROCESSOR_DIRECTIVES = { "#load-if-defined", "#load-if-not-defined", "#else", "#end-if" };
    }
}
