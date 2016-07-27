﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Common
{
    static public class MessageType
    {

        public enum Types : short
        {

            REQUEST_SIGNUP=100,
            REQEUST_CHECK_SIGNEDUP=110,

            CHAT_MSG=200,

            REQUEST_CREATE_ROOM=310,
            REQUEST_LEAVE_ROOM=320,
            REQUEST_JOIN_ROOM=330,
            REQUEST_LIST_ROOM=340,

            STATUS_SUCCESS=200,
            STATUS_FAIL=400,

        };
        /*
        public static short Header = 1;
        public static short Message = 2;

        public static short _REQUEST_SIGNUP = 100;
        public static short _REQUEST_CHECK_SIGNEDUP = 110;

        public static short _CHAT_MSG = 200;

        public static short _REQUEST_CREATE_ROOM = 310;
        public static short _REQUEST_LEAVE_ROOM = 320;
        public static short _REQUEST_JOIN_ROOM = 330;
        public static short _REQUEST_LIST_ROOM = 340;

        public static short _STATUS_SUCCESS = 200;
        public static short _STATUS_FAIL = 400;
        */

    }

   
}
