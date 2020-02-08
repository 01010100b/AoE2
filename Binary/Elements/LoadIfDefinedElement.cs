using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    // #load-if-defined or #load-if-not-defined
    class LoadIfDefinedElement : Element
    {
        public override int RuleLength => _RuleLength;
        public bool Transformed { get; set; } = false;
        private int _RuleLength = 0;

        public LoadIfDefinedElement(string code) : base(code) { }

        public void SetRuleLength(int length)
        {
            _RuleLength = length;
        }
    }
}
