using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Frame
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct RequestMessage
    {
        Header Header;
        Body Body;

        public RequestMessage(Header header, Body body)
        {
            this.Header = header;
            this.Body = body;
        }
    }
}
