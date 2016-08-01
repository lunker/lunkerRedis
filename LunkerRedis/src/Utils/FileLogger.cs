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
        public static ILog log = LogManager.GetLogger(MyConst.Logger); 
        
    }
}
