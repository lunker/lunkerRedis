using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Common
{
    public static class RedisKey
    {
        public static string FE = "fe";
        public static char DELIMITER = ':';
        
        public static string FE_List = "fe:list";
        public static string Login = "login";
        public static string ChattingRoomList = "chattingroomlist";

        public static string Room = "room";
        public static string Count = "count";

        public static string User = "user";

        public static string Ranking_Chatting = "ranking:chatting";

        public static string Dummy = "dummy";

    }
}
