using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    // (defrule
    class RuleElement : Element
    {
        public override int RuleLength => 1;

        public RuleElement(string code) : base(code) { }
    }
}
