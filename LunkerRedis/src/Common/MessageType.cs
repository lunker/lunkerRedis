using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Common
{
    static class MessageType
    {
        public static short _REQUEST_SIGNUP = 100;
        public static short _REQUEST_CHECK_SIGNEDUP = 110;

        public static short _CHAT_MSG = 200;

        public static short _REQUEST_CREATE_ROOM = 310;
        public static short _REQUEST_LEAVE_ROOM = 320;
        public static short _REQUEST_JOIN_ROOM = 330;
        public static short _REQUEST_LIST_ROOM = 340;

        public static short _STATUS_SUCCESS = 200;
        public static short _STATUS_FAIL = 400;
    }
}
