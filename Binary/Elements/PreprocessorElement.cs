using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    class PreprocessorElement : Element
    {
        public override int RuleLength => 0;

        public PreprocessorElement(string code) : base(code) { }
    }
}
