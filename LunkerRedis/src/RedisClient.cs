using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;



namespace LunkerRedis.src
{
     
    class RedisClient
    {
        const string IP = "192.168.56.102";
        const int PORT = 6379;
        private ConnectionMultiplexer redis = null;


        public void Start()
        {
            
        }

        /*
         * Connect to redis server
         */
        public bool Connect()
        {
            try
            {
                redis = ConnectionMultiplexer.Connect("192.168.56.102:6379"+ ",allowAdmin=true,password=ldk201120841");
                IDatabase db = redis.GetDatabase();


                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }// end method
    }
}
