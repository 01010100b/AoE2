using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptVM
{
    abstract class Block
    {
        public readonly List<Variable> LocalVariables;
    }
}
