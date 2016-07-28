using LunkerRedis.src.Common;
using LunkerRedis.src.Frame;
using LunkerRedis.src.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src
{
    class ClientHandler
    {
        private Socket peer = null;

        public ClientHandler() { }
        public ClientHandler(Socket handler)
        {
            this.peer = handler;
        }


        /**
         * Monitoring Client의 request 처리 
         */
        public void HandleRequest()
        {
            // Read Request
            Header header;
            MessageFrame message;

            Object obj = Parser.Read(peer,   Marshal.SizeOf(typeof(Header)), typeof(Header));

            if (obj == null)
            {
                Console.WriteLine("[HandleRequest] read header error");
            }
            else
            {
                header = (Header)obj;
                Console.WriteLine("[HandleRequest] read header success");

                //Console.WriteLine("Message marshal size" + Marshal.SizeOf(typeof(Message)));
                Console.WriteLine("Message size from header " + header.BodyLen);
                //Console.WriteLine("Message size from marshal " + Marshal.SizeOf(MessageFrame));

                message = (MessageFrame) Parser.Read(peer, header.BodyLen, typeof(MessageFrame));

                //Console.WriteLine("Client say : " + Encoding.UTF8.GetString(Encoding.Unicode.GetBytes(message.Content)));

                Console.WriteLine("Client say : " + message.Content);
            }

            // Logic 

            // Send Response
            //Parser.Send();
        }// end method 

    }// end class



}
