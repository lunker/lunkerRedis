using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunkerRedis.src;
using log4net;
using log4net.Config;
using System.Reflection;
using LunkerRedis.src.Common;
using System.Windows;
using LunkerRedis.src.Utils;
using System.Runtime.InteropServices;

namespace LunkerRedis
{
    class Program 
    {
        // Define a static logger variable so that it references the
        // Logger instance named "MyApp".
        //private static readonly ILog log = LogManager.GetLogger("Logger");
        private static ILog logger = FileLogger.GetLoggerInstance();
        private static bool isclosing = false;

        static void Main(string[] args)
        {
            logger.Debug("--------------------------------------------Start Program-----------------------------------------------------");

            SetConsoleCtrlHandler(ConsoleCtrlCheck, true);

            Backend be = new Backend();
            be.Start();
        }// end method

        #region unmanaged
        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        #endregion

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // Put your own handler here
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    isclosing = true;

                    RedisClient.RedisInstance.ClearDB();
                    RedisClient.RedisInstance.Release();
                    MySQLClient.Instance.Release();

                    logger.Debug("--------------------------------------------Exit Program-----------------------------------------------------");
                    Environment.Exit(0);
                    break;

                case CtrlTypes.CTRL_BREAK_EVENT:
                    isclosing = true;
                    Console.WriteLine("CTRL+BREAK received!");
                    break;

                case CtrlTypes.CTRL_CLOSE_EVENT:
                    isclosing = true;

                    RedisClient.RedisInstance.ClearDB();
                    RedisClient.RedisInstance.Release();
                    MySQLClient.Instance.Release();

                    logger.Debug("--------------------------------------------Exit Program-----------------------------------------------------");
                    Environment.Exit(0);
                    break;

                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    isclosing = true;
                    Console.WriteLine("User is logging off!");
                    break;
            }
            return true;
        }

    }
}