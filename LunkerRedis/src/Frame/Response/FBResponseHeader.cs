using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Frame.Response
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct FBResponseHeader
    {
        private short _type;
        private int _bodyLen;
        private short _status;

        public FBResponseHeader(short type, int bodyLen, short status)
        {
            this._type = type;
            this._bodyLen = bodyLen;
            this._status = status;
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
