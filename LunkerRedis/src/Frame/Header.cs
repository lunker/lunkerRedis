using System;
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
        private short _type;
        private int _bodyLen;

        public Header(short type, int bodyLen)
        {
            this._type = type;
            this._bodyLen = bodyLen;
        }

        public short Type
        {
            get { return this._type; }
            set { this._type = value; }
        }
       
        public int BodyLen
        {
            get { return this._bodyLen; }
            set { this._bodyLen = value; }
        }
    }
}
