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
        private Socket listener = null;
        
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
            Console.WriteLine("[sock_listener] Listen . . .");
            listener.Listen(BACK_LOG);

            try
            {
                
                while (true)
                {
                    Socket peer = null;
                    peer = listener.Accept();

                    if (PORT == MyConst.CLIENT_PORT)
                    {
                        ClientHandler handler = new ClientHandler(peer);

                        Thread clientThread = new Thread(new ThreadStart(handler.HandleRequest));
                        clientThread.Start();
                    }
                    else
                    {
                        FrontendHandler handler = new FrontendHandler(peer);
                        
                        Thread frontendThread = new Thread(new ThreadStart(handler.HandleRequest));
                        frontendThread.Start();
                    }
                }// end while
            }
            catch (SocketException se)
            {
                Console.WriteLine("[sock_listener] exception . . .");
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
            //Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.);
            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                host = new IPEndPoint(IPAddress.Any, PORT);
                listener.Bind(host);
                return listener.IsBound;
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
