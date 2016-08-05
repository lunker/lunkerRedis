using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using LunkerRedis.src.Common;

namespace LunkerRedis.src.Utils
{
    public static class FileLogger
    {
        public static ILog logger = null;

        /// <summary>
        /// Get Logger Instance
        /// </summary>
        /// <returns>logger</returns>
        public static ILog GetLoggerInstance()
        {
            if (logger == null)
            {
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(MyConst.LoggerConfigPath));
                logger = LogManager.GetLogger(MyConst.Logger);
            }
                
            return logger;
        }
    }
}
