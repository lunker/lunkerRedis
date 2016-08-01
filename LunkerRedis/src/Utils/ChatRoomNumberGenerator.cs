using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunkerRedis.src.Utils
{
    public static class ChatRoomNumberGenerator
    {
        public static int roomNo = 0;
        public static int GenerateRoomNo()
        {
            Interlocked.Increment(ref roomNo);
            return roomNo;
        }
    }
}
