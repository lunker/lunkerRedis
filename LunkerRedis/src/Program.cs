using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunkerRedis.src;



namespace LunkerRedis
{
    class Program
    {


        static void Main(string[] args)
        {
            Console.WriteLine("test");

            RedisClient redisClient = new RedisClient();
            if (redisClient.Connect())
            {
                Console.WriteLine("[Connect] success");
            }
            else
            {
                Console.WriteLine("[Connect] fail");
            }

            Console.ReadLine();// wait

        }// end method


    }
}






