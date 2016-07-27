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
        private Socket handler = null;

        public ClientHandler() { }
        public ClientHandler(Socket handler)
        {
            this.handler = handler;
        }

       

    }// end class



}
