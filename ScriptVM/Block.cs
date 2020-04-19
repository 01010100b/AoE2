using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptVM
{
    abstract class Block : Statement
    {
        public readonly List<Variable> LocalVariables;
        public readonly List<Statement> Statements;
    }
}
