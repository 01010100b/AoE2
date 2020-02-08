using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    class LoadRandomElement : Element
    {
        public override int RuleLength => 0;

        public LoadRandomElement(string code) : base(code) { }
    }
}
