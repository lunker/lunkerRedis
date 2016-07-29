using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Common
{
    public static class HeaderLength
    {
        public enum Lengths : int
        {
            FBChatRequestBody=4, // id
            FBLoginRequestBody=-1,
            FBRoomRequestBody=-1,
            
            FBLoginResponseBody = -1,
            FBRoomResponseBody = -1,
             
        };
    }
}
