using log4net;
using LunkerRedis.src.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src
{

    public static class MySQLClientPool
    {
        private static ILog logger = FileLogger.GetLoggerInstance();
        private static int poolSize = 10;
        private static List<MySQLClient> _available = new List<MySQLClient>();
        private static List<MySQLClient> _inUse = new List<MySQLClient>();

        /// <summary>
        /// Generate Pool
        /// </summary>
        static MySQLClientPool()
        {
            for (int idx = 0; idx < poolSize; idx++)
            {
                MySQLClient client = new MySQLClient();
                client.Connect();

                _available.Add(client);
            }
        }

        /// <summary>
        /// Get MySQLClient Instance
        /// </summary>
        /// <returns></returns>
        public static MySQLClient GetInstance()
        {
            logger.Debug("[MySQLClientPool] Get Instance");
            lock (_available)
            {
                if (_available.Count != 0)
                {
                    MySQLClient po = _available[0];
                    _inUse.Add(po);
                    _available.RemoveAt(0);
                    return po;
                }
                else
                {
                    MySQLClient po = new MySQLClient();
                    _inUse.Add(po);
                    return po;
                }
            }
            return null;
        }// end method

        public static void ReleaseObject(MySQLClient po)
        {
            logger.Debug("[MySQLClientPool] Release Instance");
            //CleanUp(po);
            lock (_available)
            {
                _available.Add(po);
                _inUse.Remove(po);
            }
        }

        public static void Dispose()
        {
            logger.Debug("[MySQLClientPool] Clear All Instances");

            /*
            foreach (MySQLClient client in _available)
            {
                client.Release();
            }
            */


            for(int idx=0; idx<poolSize; idx++)
            {
                if (_available.ElementAt(idx) != null)
                {
                    _available.ElementAt(idx).Release();
                }
            }

            _available.Clear();
            _available = null;
        }

    }

}
