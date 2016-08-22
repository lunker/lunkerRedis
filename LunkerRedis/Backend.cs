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

        private bool appState = MyConst.Run;
        public void Start()
        {
            while (threadState)
            {
                if (frontendListener == null)
                {
                    StartFrontendListener();
                }

                /*
                if(clientListener == null)
                {
                    StartClientListener();
                }
                */


                Console.Write("어플리케이션을 종료하시겠습니까? (y/n) : ");
                string close = Console.ReadLine();
                if (close.Equals("y") || close.Equals("Y"))
                {
                    //backendServer.RequestStopThread();// stop all thread
                    RedisClientPool.GetInstance().ClearDB(); // clear db
                    RedisClientPool.Dispose(); // release object pool
                    MySQLClientPool.Dispose(); // release object pool
                    threadState = MyConst.Exit; // exit main thread

                    Console.Clear();
                    Console.Write("어플리케이션을 종료중입니다 . . .");
                    logger.Debug("--------------------------------------------Exit Program-----------------------------------------------------");
                    Environment.Exit(0); // exit main process
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("다시 입력하십시오.");
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
            //IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any);
            if (frontendListener.Connect())
            {
                Console.WriteLine("[FE_LISTENER] conenct success");
                logger.Debug("[FE_LISTENER] conenct success");
            }

            else
            {
                Console.WriteLine("[FE_LISTENER] conenct fail");
                logger.Debug("[FE_LISTENER] conenct fail");
            }
                

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
