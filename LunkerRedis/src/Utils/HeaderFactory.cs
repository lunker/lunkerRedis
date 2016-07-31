using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Utils
{
    public static class HeaderFactory
    {

        public static Object CreateInstance(Type type, params object[] list )
        {

            if(type == typeof(CFHeader))
            {
                // CFHEADER
                FBHeader header = new FBHeader();
                header.Type = 
                
            }
            else
            {
                // FBHEADER
            }
        }
    }
}
