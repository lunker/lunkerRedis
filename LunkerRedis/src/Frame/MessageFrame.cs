using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Frame
{

    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Unicode) ]
    public struct MessageFrame
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        private string _content;

        public MessageFrame(string content) {
            this._content = content;
        }

        public string Content
        {
            get { return this._content; }
            set { this._content = value; }
        }

        /*
        [MarshalAs(UnmanagedType.HString)]
        private byte[] _content;

        public MessageFrame(byte[] _content)
        {
            this._content = _content;
        }

        public byte[] Content
        {
            get { return this._content; }
            set { this._content = value; }
        }
        */

    }
}
