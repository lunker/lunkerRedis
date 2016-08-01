using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Common
{
    static public class MessageType
    {
        public enum FBMessageType : short
        {
            Id_Dup = 110,
            Signup = 120,

            Login = 210,

            Room_Create = 310,
            Room_Leave = 320,
            Room_Join = 330,
            Room_List = 340,

            Chat_Count = 410
        };

        public enum FBMessageState : short
        {
            REQUEST = 100,
            SUCCESS = 200,
            FAIL = 400
        }
     

    }// end class

   
}
