using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using LunkerRedis.src.Utils;

namespace LunkerRedis.src
{
    public static class RedisClientPool
    {
        private static ILog logger = FileLogger.GetLoggerInstance();
        private static int poolSize = 10;
        private static List<RedisClient> _available = new List<RedisClient>();
        private static List<RedisClient> _inUse = new List<RedisClient>();

        /// <summary>
        /// Generate Pool
        /// </summary>
        static RedisClientPool()
        {
            for(int idx=0; idx<poolSize; idx++)
            {
                RedisClient client = new RedisClient();
                client.Connect();

                _available.Add(client);
            }
        }

        /// <summary>
        /// Get RedisClient Instance
        /// </summary>
        /// <returns></returns>
        public static RedisClient GetInstance()
        {
            logger.Debug("[RedisClientPool] Get Instance");
            lock (_available)
            {
                if (_available.Count != 0)
                {
                    RedisClient po = _available[0];
                    _inUse.Add(po);
                    _available.RemoveAt(0);
                    return po;
                }
                else
                {
                    RedisClient po = new RedisClient();
                    _inUse.Add(po);
                    return po;
                }
            }
         
        }// end method
    
        public static void ReleaseObject(RedisClient po)
        {
            logger.Debug("[RedisClientPool] Release Instance");
            //CleanUp(po);
            lock (_available)
            {
                _available.Add(po);
                _inUse.Remove(po);
            }
        }

        public static void Dispose()
        {
            logger.Debug("[RedisClientPool] Clear All Instances");

            for (int idx = 0; idx < poolSize; idx++)
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
