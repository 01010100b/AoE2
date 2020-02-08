using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    class DefStrategicElement : Element
    {
        public override int RuleLength => 0;
        public string Name { get; private set; }

        public DefStrategicElement(string code) : base(code)
        {
            var pieces = Pieces;

            Name = pieces[1].Trim();
        }
    }
}
