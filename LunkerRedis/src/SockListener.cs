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

using log4net;


namespace LunkerRedis.src
{
    class SockListener
    {

        private ILog logger = FileLogger.GetLoggerInstance();
        private Socket listener = null;
        
        private int PORT = default(int);// host port
        private string IP = default(string); // host ip

        private int BACK_LOG = 1000;
        private bool threadState = MyConst.Run;

        private List<FrontendHandler> frontendHandlerList = null; // frontend handler list
        private List<ClientHandler> clientHandlerList = null; // client handler list


        public SockListener() { }
        public SockListener(string ip, int port)
        {
            this.IP = ip;
            this.PORT = port;
            frontendHandlerList = new List<FrontendHandler>();
            clientHandlerList = new List<ClientHandler>();
        }

        public void Listen()
        {
            listener.Listen(BACK_LOG);

            try
            {
                while (threadState)
                {
                    Socket peer = null;
                    peer = listener.Accept();

                    if (PORT == MyConst.clientPort)
                    {
                        ClientHandler handler = new ClientHandler(peer);

                        Thread clientThread = new Thread(new ThreadStart(handler.HandleRequest));
                        clientHandlerList.Add(handler);
                        clientThread.Start();
                    }
                    else
                    {
                        FrontendHandler handler = new FrontendHandler(peer);
                        
                        Thread frontendThread = new Thread(new ThreadStart(handler.HandleRequest));
                        frontendHandlerList.Add(handler);
                        frontendThread.Start();
                    }
                }// end while
            }
            catch (SocketException se)
            {
                return;
            }
        }// end method
        
        public void RequestStopThread()
        {
            logger.Debug("[SockListener][RequestStopThread()] Stop Handler Thread~!");
            this.threadState = MyConst.Exit;

            foreach (FrontendHandler handler in frontendHandlerList)
            {
                handler.HandleStopThread();
            }

            foreach (ClientHandler handler in clientHandlerList)
            {
                handler.HandleStopThread();
            }

        }

        /// <summary>
        /// Bind Socket
        /// </summary>
        /// <returns></returns>
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

                return false;
            }
            catch (ArgumentNullException ane)
            {
                return false;   
            }
        }// end method
    }
}
