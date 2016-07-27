﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Frame
{

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct Message
    {
        char[] userId;
        char[] content;
    }
}
