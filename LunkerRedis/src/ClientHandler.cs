using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        }


    }// end class



}
