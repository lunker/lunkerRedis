using log4net;
using LunkerRedis.src.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src
{
    /// <summary>
    /// Object Pool for MySQLClient
    /// </summary>
    public static class MySQLClientPool
    {
        private static ILog logger = FileLogger.GetLoggerInstance();
        private static int poolSize = 5; // max pool size
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
            
            lock (_available)
            {
                logger.Debug("[MySQLClientPool] Get Instance");
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

        /// <summary>
        /// Release MySQLClient Instance
        /// </summary>
        /// <param name="po"></param>
        public static void ReleaseObject(MySQLClient po)
        {
            
            //CleanUp(po);
            lock (_available)
            {
                logger.Debug("[MySQLClientPool] Release Instance");
                _available.Add(po);
                _inUse.Remove(po);
            }
        }

        /// <summary>
        /// Dispose MySQLClient Pool
        /// </summary>
        public static void Dispose()
        {
            logger.Debug("[MySQLClientPool] Clear All Instances");
            logger.Info("[RedisClientPool] Clear All Instances");

            /*
            foreach (MySQLClient client in _available)
            {
                client.Release();
            }
            */
            lock (_available)
            {
                if (_available != null && _available.Count != 0)
                {
                 
                    foreach (MySQLClient client in _available)
                    {
                        client.Release();
                    }
                    _available.Clear();
                    //_available = null;

                }
            }

        }

    }

}
