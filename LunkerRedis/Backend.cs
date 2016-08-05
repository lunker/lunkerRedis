using LunkerRedis.src;
using LunkerRedis.src.Frame;
using LunkerRedis.src.Utils;
using LunkerRedis.src.Common;

using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Windows;

using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;


namespace LunkerRedis
{
    class Backend
    {
        private SockListener frontendListener = null; // Socket Listener For Frontend Server
        private SockListener clientListener = null;
        private ILog logger = FileLogger.GetLoggerInstance();

        public void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            // connection for FE 
            frontendListener = new SockListener(MyConst.IP,MyConst.FRONTEND_PORT);
            if (frontendListener.Connect())
                logger.Debug("[FE_LISTENER] conenct success");
            else
                logger.Debug("[FE_LISTENER] conenct fail");

            Thread fListenerThread = new Thread(new ThreadStart(frontendListener.Listen));
            fListenerThread.Start();
         
            // connection for Monitoring Server
            clientListener = new SockListener(MyConst.IP, MyConst.CLIENT_PORT);
            if (clientListener.Connect())
                logger.Debug("[CLINET_LISTENER] conenct success");
            else
                logger.Debug("[CLIENT_LISTENER] conenct fail");

            Thread cListenerThread = new Thread(new ThreadStart(clientListener.Listen));
            cListenerThread.Start();
        }

        public void RequestStopThread()
        {
            frontendListener.RequestStopThread();
            clientListener.RequestStopThread();
        }
    }
}
