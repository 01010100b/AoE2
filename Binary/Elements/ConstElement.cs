using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    // (defconst
    class ConstElement : Element
    {
        public override int RuleLength => 0;
        public string Name { get; private set; }
        public string Value { get; private set; }

        public ConstElement(string code) : base(code)
        {
            var pieces = Pieces;
            Debug.Assert(pieces.Count == 3, "invalid defconst: " + code);

            Name = pieces[1].Trim();
            Value = pieces[2].Replace(")", "").Trim();
        }

        public void SetValue(string value)
        {
            Code = Code.Replace(Value, value);
            Value = value;
        }
    }
}
