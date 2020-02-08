using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    // whitespace or starts with ;
    class CommentElement : Element
    {
        public override int RuleLength => 0;

        public CommentElement(string code) : base(code) { }
    }
}
