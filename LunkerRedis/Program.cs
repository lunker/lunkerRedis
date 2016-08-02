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

namespace LunkerRedis
{
    class Program
    {
        // Define a static logger variable so that it references the
        // Logger instance named "MyApp".
        //private static readonly ILog log = LogManager.GetLogger("Logger");

        static void Main(string[] args)
        {

            /*
            // Set up a simple configuration that logs on the console.
           

            ILog log = LogManager.GetLogger(MyConst.Logger);
            log.Info("Entering application.");
            */
            


            Backend be = new Backend();
            be.Start();
        }// end method
    }
}