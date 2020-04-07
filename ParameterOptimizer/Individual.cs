using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParameterOptimizer
{
    class Individual
    {
        public readonly Dictionary<string, int> Parameters = new Dictionary<string, int>();
        public int Score { get; set; } = -1;
    }
}
