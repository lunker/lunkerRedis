using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using LunkerRedis.src.Utils;
using LunkerRedis.src.Common;
using LunkerRedis.src.Frame;


namespace LunkerRedis.src
{
    class FrontendHandler
    {
        private Socket peer = null;

        public FrontendHandler() { }

        public FrontendHandler(Socket handler)
        {
            this.peer = handler;
        }

        /**
         * Frontend Server의 request 처리 
         */
        public void HandleRequest()
        {
            while (true)
            {

                // Read Request
                Header header = (Header) Parser.Read(peer, MyConst.HEADER_LENGTH, typeof(Header));
                
                

                // Logic 
                

                // Send Response
                //Parser.Send();
            }
        }


        
        

    }
}
