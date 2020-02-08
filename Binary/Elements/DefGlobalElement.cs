using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    class DefGlobalElement : Element
    {
        public override int RuleLength => 0;
        public string Name { get; private set; }

        public DefGlobalElement(string code) : base(code)
        {
            var pieces = Pieces;

            Name = pieces[1].Trim();
        }
    }
}
