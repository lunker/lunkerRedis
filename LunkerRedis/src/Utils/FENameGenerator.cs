using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Threading.Tasks;

namespace LunkerRedis.src.Utils
{
    public static class FENameGenerator
    {
        public static int num = 1;
        public static int GenerateName()
        {
            Interlocked.Increment(ref num);
            return num;
        }

    }
}
