using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    // #deflocal
    class DefLocalElement : Element
    {
        public override int RuleLength => 0;
        public string Name { get; private set; }
        public string Initial { get; private set; }
        public int Index { get; set; } = -1;
        public int FreeIndex { get; set; } = 0;

        public DefLocalElement(string code) : base(code)
        {
            var pieces = Pieces;
            Name = Pieces[1];

            Initial = null;
            if (pieces.Count > 3)
            {
                Initial = pieces[3];
            }
        }

        public void SetName(string name)
        {
            Code = Code.Replace(Name, name);
            Name = name;
        }
    }
}
