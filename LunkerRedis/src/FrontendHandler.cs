using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;



namespace LunkerRedis.src
{
    class FrontendHandler
    {
        

        private Socket handler = null;

        public FrontendHandler() { }
        public FrontendHandler(Socket handler)
        {
            this.handler = handler;
        }
    }

    


}
