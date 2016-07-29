using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using LunkerRedis.src.Utils;
using LunkerRedis.src.Common;
using LunkerRedis.src.Frame;
using System.Runtime.InteropServices;

namespace LunkerRedis.src
{
    class FrontendHandler
    {
        private Socket Peer = null;
        private RedisClient redis = null;

        public FrontendHandler() { }

        public FrontendHandler(Socket peer)
        {
            this.Peer = peer;
        }

        /**
         * Frontend Server의 request 처리 
         */
        public void HandleRequest()
        {

            redis = new RedisClient();
            redis.Connect();

            while (true)
            {
                // Read Request
                FBHeader header;
                byte[] bodyArr = null;

                header = (FBHeader)Parser.Read(Peer, (int)ProtoclHeaderLength.FBHeader , typeof(FBHeader));

                switch (header.type)
                {
<<<<<<< HEAD
               
=======
                    case (short)FBMessageType.Id_Dup:
                        // 아이디 중복 확인 요청 
                        


                        break;
                    case (short)FBMessageType.Signup:
                        // 회원가입 요청 

                        break;
                    case (short)FBMessageType.Login:
                        // 로그인 요청 
                        break;
                    case (short)FBMessageType.Room_Create:
                        // 채팅방 생성 : 끝 
                    

                        int result = redis.CreateChatRoom();
                        /*
                         * Body가 정의되어 있지 않음 . . .
                         */
                        // 4byte int 를 byte[]로 전송.
                        // send result
                        Parser.Send(Peer, BitConverter.GetBytes(result));
                        break;
                    case (short)FBMessageType.Room_Leave:
                        // 채팅방 나가기 : 
                        break;
                    case (short)FBMessageType.Room_Join:
                        // 채팅방 입장 : 

                        break;
                    case (short)FBMessageType.Room_List:
                        // 채팅방 목록 조회 : 

                        break;
                    case (short)FBMessageType.Chat_Count:
                        break;

>>>>>>> e6adf318ffbfb2d258efc2d41e4ae2b8f7e07f6a
                    default:

                        break;
                }// end switch
            }//end loop
        }


        
        

    }
}
