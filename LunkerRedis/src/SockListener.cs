using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LunkerRedis.src.Common;
using LunkerRedis.src.Frame;
using LunkerRedis.src.Utils;

namespace LunkerRedis.src
{

    /**
     * socket listener
     * 
     */ 
    class SockListener
    {
        private Socket Listener = null;
        
        private int PORT = default(int);// host port
        private string IP = default(string); // host ip

        private int BACK_LOG = 1000;

        public SockListener() { }
        public SockListener(string ip, int port)
        {
            this.IP = ip;
            this.PORT = port;
        }

        /*
         * 
         */
        public void Listen()
        {
            Socket peer = null;

            Listener.Listen(BACK_LOG);
            Console.WriteLine("[LISTENER] Listen");
            try
            {
                while (true)
                {
                    peer = Listener.Accept();

                    if(PORT == MyConst.CLIENT_PORT)
                    {
                        ClientHandler handler = new ClientHandler(peer);
                        
                        Thread clientThread = new Thread(new ThreadStart(handler.HandleRequest));
                        clientThread.Start();
                    }
                    else
                    {
                        FrontendHandler handler = new FrontendHandler(peer);

                        Thread clientThread = new Thread(new ThreadStart(handler.HandleRequest));
                        clientThread.Start();
                    }
                  
                }// end while
            }
            catch (SocketException se)
            {

            }
        }// end method
        
        /*
         * Host' IP-PORT로  socket connect
         * <return> bool 
         * true:  
         * false: 
         */ 
        public bool Connect() 
        {
            IPEndPoint host = null;

            Listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                host = new IPEndPoint(IPAddress.Parse(IP), PORT);
                Listener.Bind(host);
                return Listener.IsBound;
            }
            catch(SocketException se)
            {
                Console.WriteLine(se.SocketErrorCode);
                return false;
            }
            catch (ArgumentNullException ane)
            {
                return false;   
            }
        }// end method
    }
}
