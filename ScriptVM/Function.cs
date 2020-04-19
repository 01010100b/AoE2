using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptVM
{
    class Function
    {
        public readonly VirtualType ReturnType;
        public readonly List<Variable> Parameters;
        public readonly Block Code;
    }
}
