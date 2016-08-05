using System;
using LunkerRedis.src;
using log4net;
using LunkerRedis.src.Common;
using LunkerRedis.src.Utils;
using System.Xml;

namespace LunkerRedis
{
    class Program
    {
        private static ILog logger = FileLogger.GetLoggerInstance();
        private static Backend backendServer = null;
        private static bool appState = MyConst.Run;

        static void Main(string[] args)
        {
            logger.Debug("\n\n\n\n\n--------------------------------------------Backend Server-----------------------------------------------------");
            logger.Debug("--------------------------------------------Start Program-----------------------------------------------------");

            backendServer = new Backend();
            backendServer.Start();

            while (appState)
            {
                Console.Write("어플리케이션을 종료하시겠습니까? (y/n) : ");
                string close = Console.ReadLine();
                if (close.Equals("y") || close.Equals("Y"))
                {
                    backendServer.RequestStopThread();
                    RedisClientPool.GetInstance().ClearDB();
                    RedisClientPool.Dispose();
                    MySQLClientPool.Dispose();
                    appState = MyConst.Exit;

                    Console.Clear();
                    Console.Write("어플리케이션을 종료중입니다 . . .");
                    logger.Debug("--------------------------------------------Exit Program-----------------------------------------------------");
                    Environment.Exit(0);
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("다시 입력하십시오.");
                }
            }
        }// end method
    }
}