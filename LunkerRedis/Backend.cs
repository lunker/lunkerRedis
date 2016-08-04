﻿using LunkerRedis.src;
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
        private SockListener frontendListener = null;
        private SockListener clientListener = null;
        private ILog logger = FileLogger.GetLoggerInstance();

        //wha
        public void Start()
        {
            Initialize();

            Console.WriteLine(Console.ReadLine());
            //Console.ReadLine();// wait
            
        }

        public void Initialize()
        {

            
            // connection for FE 
            frontendListener = new SockListener(MyConst.IP,MyConst.FRONTEND_PORT);
            if (frontendListener.Connect())
                Console.WriteLine("[FE_LISTENER] conenct success");
            else
                Console.WriteLine("[FE_LISTENER] conenct fail");

            Thread fListenerThread = new Thread(new ThreadStart(frontendListener.Listen));
            fListenerThread.Start();
            Console.WriteLine("[FE_HANDLER] 초기화 완료");

         
            // connection for Monitoring Server
            clientListener = new SockListener(MyConst.IP, MyConst.CLIENT_PORT);
            if(clientListener.Connect())
                Console.WriteLine("[CLINET_LISTENER] conenct success");
            else
                Console.WriteLine("[CLIENT_LISTENER] conenct fail");

            Thread cListenerThread = new Thread(new ThreadStart(clientListener.Listen));
            cListenerThread.Start();
            Console.WriteLine("[CLIENT_HANDLER] 초기화 완료");

            while (true)
            {
                switch (cListenerThread.ThreadState)
                {
                    case ThreadState.Aborted:
                        
                    case ThreadState.Stopped:
                        Console.WriteLine("asdfasdf");
                        logger.Info("Client Thread is Stopped");
                        break;

                }

            }
            //cListenerThread.Join();

        }
    }
}
