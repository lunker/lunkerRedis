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
        private Socket peer = null;
        private RedisClient redis = null;
        private MySQLClient mysql = null;

        public FrontendHandler() { }

        public FrontendHandler(Socket peer)
        {
            this.peer = peer;
        }

        /**
         * Frontend Server의 request 처리 
         */
        public void HandleRequest()
        {

            redis = new RedisClient();
            redis.Connect();
            mysql = new MySQLClient();
            mysql.Connect();

            while (true)
            {
                // Read Request
                FBHeader header;
                byte[] bodyArr = null;

                header = (FBHeader)Parser.Read(peer, (int)ProtocolHeaderLength.FBHeader , typeof(FBHeader));

                switch (header.type)
                {
                    case FBMessageType.Id_Dup:
                        // 아이디 중복 확인 요청 
                        


                        break;
                    case FBMessageType.Signup:
                        // 회원가입 요청 

                        // read body

                        HandleCreateUser();

                        // generate Header


                        //mysql.CreateUser(body.Id, body.Password);



                        break;
                    case FBMessageType.Login:


                        // 로그인 요청 
                        break;
                    case FBMessageType.Room_Create:
                        // 채팅방 생성 : 끝 
                        HandleCreateChatRoom();

                        break;
                    case FBMessageType.Room_Leave:
                        // 채팅방 나가기 : 
                        break;
                    case FBMessageType.Room_Join:
                        // 채팅방 입장 : 

                        break;
                    case FBMessageType.Room_List:
                        // 채팅방 목록 조회 : 

                        break;
                    case FBMessageType.Chat_Count:
                        break;


                    default:

                        break;
                }// end switch
            }//end loop
        }// end method 

        public void HandleCreateUser()
        {
            FBSignupRequestBody body = (FBSignupRequestBody)Parser.Read(peer, (int)ProtocolBodyLength.FBSignupRequestBody, typeof(FBSignupRequestBody));
            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 

            bool result =  mysql.CreateUser(id, password);

            if (result)
            {
                FBHeader header = new FBHeader();

                // send result
                Parser.Send(peer, header);
            }
            else
            {
                ;
            }

        } 

        public void HandleLogin()
        {
            FBSignupRequestBody body = (FBSignupRequestBody)Parser.Read(peer, (int)ProtocolBodyLength.FBSignupRequestBody, typeof(FBSignupRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 

            bool result = mysql.Login(id, password);
        }

        public void HandleCreateChatRoom()
        {

            int result = redis.CreateChatRoom();
            /*
             * Body가 정의되어 있지 않음 . . .
             */
            // 4byte int 를 byte[]로 전송.
            // send result
            Parser.Send(peer, BitConverter.GetBytes(result));
        }
    }
}
