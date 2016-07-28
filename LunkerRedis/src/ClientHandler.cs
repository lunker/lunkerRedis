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
            Message message;

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

                message = (Message) Parser.Read(peer, header.BodyLen, typeof(Message));

                Console.WriteLine("Client say : " + message.Content);
            }

            // Logic 

            // Send Response
            //Parser.Send();
        }// end method 

    }// end class



}
