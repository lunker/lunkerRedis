using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using LunkerRedis.src.Utils;

namespace LunkerRedis.src
{
    /// <summary>
    /// Object Pool for RedisClient 
    /// </summary>
    public static class RedisClientPool
    {
        private static ILog logger = FileLogger.GetLoggerInstance();
        private static int poolSize = 5;
        private static List<RedisClient> _available = new List<RedisClient>();
        private static List<RedisClient> _inUse = new List<RedisClient>();

        /// <summary>
        /// RedisClientPool Static Structor
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
            
            lock (_available)
            {
                logger.Debug("[RedisClientPool] Get Instance");

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

        /// <summary>
        /// Release RedisClient Instance
        /// </summary>
        /// <param name="obj">RedisClient instance</param>
        public static void ReleaseObject(RedisClient obj)
        {
            
            //CleanUp(po);
            lock (_available)
            {
                logger.Debug("[RedisClientPool] Release Instance");
                _available.Add(obj);
                _inUse.Remove(obj);
            }
        }

        /// <summary>
        /// Dispose RedisClient Pool
        /// </summary>
        public static void Dispose()
        {
            logger.Debug("[RedisClientPool] Clear All Instances");
            logger.Info("[RedisClientPool] Clear All Instances");

            lock (_available)
            {
                if (_available != null && _available.Count != 0)
                {
                    /*
                    for (int idx = 0; idx < poolSize; idx++)
                    {
                        if (_available.ElementAt(idx) != null)
                        {
                            _available.ElementAt(idx).Release();
                        }
                    }
                    */
                    foreach (RedisClient client in _available)
                    {
                        client.Release();
                    }

                    _available.Clear();
                    //_available = null;

                }
            }//end lock
        }// end method

    }
}
