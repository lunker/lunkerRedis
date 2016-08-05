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
    /// <summary>
    /// Backend Server
    /// </summary>
    class Backend
    {
        private SockListener frontendListener = null; // Socket Listener For Frontend Server
        private SockListener clientListener = null;
        private bool threadState = MyConst.Run;
        private ILog logger = FileLogger.GetLoggerInstance();

        public void Start()
        {
            while (threadState)
            {
                if(frontendListener == null)
                {
                    StartFrontendListener();
                }

                if(clientListener == null)
                {
                    StartClientListener();
                }
            }
        }
        /// <summary>
        /// Start Frontend Socket Listener Thread 
        /// </summary>
        public void StartFrontendListener()
        {

            // connection for FE 
            frontendListener = new SockListener(IPAddress.Any.ToString(), MyConst.frontendPort);
            if (frontendListener.Connect())
                logger.Debug("[FE_LISTENER] conenct success");
            else
                logger.Debug("[FE_LISTENER] conenct fail");

            Thread fListenerThread = new Thread(new ThreadStart(frontendListener.Listen));
            fListenerThread.Start();
        }
        /// <summary>
        /// Start Client Socket Listener Thread 
        /// </summary>
        public void StartClientListener()
        {
            // connection for Monitoring Server
            clientListener = new SockListener(IPAddress.Any.ToString(), MyConst.clientPort);
            if (clientListener.Connect())
                logger.Debug("[CLINET_LISTENER] conenct success");
            else
                logger.Debug("[CLIENT_LISTENER] conenct fail");

            Thread cListenerThread = new Thread(new ThreadStart(clientListener.Listen));
            cListenerThread.Start();
        }

        /// <summary>
        ///  Stop Thread 
        /// </summary>
        public void RequestStopThread()
        {
            threadState = MyConst.Exit;

            frontendListener.RequestStopThread();
            clientListener.RequestStopThread();
        }
    }
}
