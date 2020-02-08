using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Binary.Elements
{
    // (load
    class LoadElement : Element
    {
        public override int RuleLength => 0;
        public readonly string File;

        public LoadElement(string code) : base(code)
        {
            var pieces = Pieces;
            File = pieces[1].Replace(")", "").Replace("\"", "").Replace("/", "\\") + ".per";

            Assert.That(File.All(ch => !Path.GetInvalidPathChars().Contains(ch)), "File invalid: " + code);
        }
    }
}
