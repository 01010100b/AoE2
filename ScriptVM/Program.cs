using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptVM
{
    class Program
    {
        public readonly List<Variable> GlobalVariables;
        public readonly List<Function> Functions;
        public readonly Block Code;
    }
}
