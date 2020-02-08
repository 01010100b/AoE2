using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    class StatementElement : Element
    {
        public override int RuleLength => 0;

        public StatementElement(string code) : base(code) { }
    }
}
