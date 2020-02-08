using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary
{
    public static class Assert
    {
        public static void That(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }
    }
}
