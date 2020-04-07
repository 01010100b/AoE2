using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParameterOptimizer
{
    public class Parameter
    {
        public readonly string Name;
        public readonly int Min;
        public readonly int Max;

        public Parameter(string name, int min, int max)
        {
            Name = name;
            Min = min;
            Max = max;
        }
    }
}
