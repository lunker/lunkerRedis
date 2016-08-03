using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Common
{
    public static class MyConst
    {
        public static int FRONTEND_PORT = 25389;
        public static int CLIENT_PORT = 20852;

        // 10.100.58.3
        public static string IP = "10.100.58.3";
        public static string Redis_IP = "192.168.56.102";
        public static string Mysql_IP = "192.168.56.190";

        public static int Redis_Port = 6379;


        //public static string IP = "127.0.0.1";
        //public static string IP = "localhost";

        public static int HEADER_LENGTH = 4;
        public static bool LOGINED = true;
        public static bool LOGOUT = false;
        public static bool Dummy = true;
        public static bool User = false;
        public static string LoggerConfigPath = "D:\\workspace\\LunkerRedis\\LunkerRedis\\config\\Logconfig.xml";
        public static string Logger = "Logger";
    }
}
