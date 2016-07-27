using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src
{

    /**
     * socket listener
     * 
     */ 
    class SockListener
    {
        private Socket listener = null;
        
        private int PORT = default(int);// host port
        private string IP = default(string); // host ip
        private int BACK_LOG = 1000;

        public SockListener() { }
        public SockListener(int port, string ip)
        {
            this.PORT = port;
            this.IP = ip;
        }

        /*
         * 
         */
        public Socket Listen()
        {
            listener.Listen(BACK_LOG);
            listener.Accept();
            return null;
        }
        
        /*
         * Host' IP-PORT로  socket connect
         * return bool 
         * true:  
         * false: 
         */ 
        public bool Connect() 
        {
            IPEndPoint host = null;

            listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                host = new IPEndPoint(IPAddress.Parse(IP), PORT);
            }
            catch (ArgumentNullException ane)
            {
                return false;   
            }

            listener.Connect(host);

            return listener.Connected;
        }

    }
}
