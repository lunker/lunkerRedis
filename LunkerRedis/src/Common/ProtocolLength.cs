using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Common
{
    
    public enum ProtocolHeaderLength: int
    {

        FBHeader = 12,
        CBHeader = 8
    };

    public enum ProtocolBodyLength: int
    {
        FBSignupRequestBody = 20, // id: 10, password: 10 

        FBLoginRequestBody = 21, // id: 10 , password: 10 , isDummy : 1

        FBRoomRequestBody = 14, // id : 10, roomNo :4 
        FBRoomResponseBody = -1,

        FBChatRequestBody = 10, // id : 10 
    }
}

