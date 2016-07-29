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
        private Socket Peer = null;

        public FrontendHandler() { }

        public FrontendHandler(Socket peer)
        {
            this.Peer = peer;
        }

        /**
         * Frontend Server의 request 처리 
         */
        public void HandleRequest()
        {
            while (true)
            {

                // Read Request
                Header header;
                byte[] bodyArr = null;

                header = (Header)Parser.Read(Peer, MyConst.HEADER_LENGTH, typeof(Header));

                switch (header.Type)
                {
               
                    default:

                        break;
                }// end switch
            }//end loop
        }


        
        

    }
}
