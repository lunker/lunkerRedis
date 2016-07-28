using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Frame
{

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct Message
    {
        /*
        [MarshalAs(UnmanagedType.HString)]
        private byte[] _content;

        public Message(string content) {
            this._content = content;
        }
        public string Content
        {
            get { return this._content; }
            set { this._content = value; }
        }
        */

        //char[] userId;\
        
        private byte[] _content;

        public Message(byte[] _content)
        {
            this._content = _content;
        }
        public byte[] Content
        {
            get { return this._content; }
            set { this._content = value; }
        }
    }
}
