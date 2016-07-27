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
        const string REDIS_IP = "192.168.56.102";
        const int REDIS_PORT = 6379;
        private ConnectionMultiplexer _redis = null;
        private IDatabase db = null;


        public void Start()
        {
            
            
        }
        
        public ConnectionMultiplexer Redis
        {
            get { return this._redis; }
            //set { }
        }

        /*
         * Connect to redis server
         */
        public bool Connect()
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect("192.168.56.102:6379"+ ",allowAdmin=true,password=ldk201120841");
                db = _redis.GetDatabase();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }// end method

        public bool Test()
        {
            ;
        }



    }
}
