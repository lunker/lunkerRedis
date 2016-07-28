﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Frame
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct Header
    {
        short Type;
        int BodyLen;

        public Header(short type, int bodyLen)
        {
            this.Type = type;
            this.BodyLen = bodyLen;
        }
    }
}
