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
        private string remoteIP = "";

        public FrontendHandler() { }

        public FrontendHandler(Socket peer)
        {
            this.peer = peer;
            IPEndPoint ep = (IPEndPoint) peer.RemoteEndPoint;
            remoteIP = ep.Address.ToString();
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
                //byte[] bodyArr = null;

                header = (FBHeader)Parser.Read(peer, (int)ProtocolHeaderLength.FBHeader, typeof(FBHeader));

                switch (header.type)
                {
                    case FBMessageType.Id_Dup:
                        // 아이디 중복 확인 요청 
                       
                        break;
                    case FBMessageType.Signup:
                        // 회원가입 요청 
                        HandleCreateUser(header.Length);
                        break;

                    case FBMessageType.Login:
                        // 로그인 요청
                        HandleLogin();
                        break;

                    case FBMessageType.Room_Create:
                        // 채팅방 생성 : 끝 
                        HandleCreateChatRoom(header.Length);
                        break;

                    case FBMessageType.Room_Leave:
                        // 채팅방 나가기 : 
                        HandleLeaveRoom();
                        break;
                    case FBMessageType.Room_Join:
                        // 채팅방 입장 : 

                        break;
                    case FBMessageType.Room_List:
                        // 채팅방 목록 조회 : 
                        HandleListRoom(header.Length);
                        break;
                    case FBMessageType.Chat_Count:
                        break;


                    default:

                        break;
                }// end switch
            }//end loop
        }// end method 

        /*
         * Handle CrateUser 
         * 
         */
        public void HandleCreateUser(int bodyLength)
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
        
        /*
         * Handle Login 
         * 1) DB를 통해서 사용자 정보 확인 
         * 2) user의 정보 cache 
         * 3) FE에 접속 했음을 표시. 
         */ 
        public void HandleLogin()
        {
            // read body
            FBLoginRequestBody body = (FBLoginRequestBody)Parser.Read(peer, (int)ProtocolBodyLength.FBLoginRequestBody, typeof(FBLoginRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 

            User result = mysql.SelectUserInfo(id);

            FBHeader header = new FBHeader();
            header.Type = FBMessageType.Login;
            header.Length = 0;

            // 로그인 성공 
            if (result.Password.Equals(password))
            {
                header.State = FBMessageState.SUCCESS;

                // add user info to cache
                redis.AddUserCache(result.Id, result.NumId);
            }
            else
            {
                header.State = FBMessageState.FAIL;
            }

            // send result 
            Parser.Send(peer, header);

        }

        public void HandleCreateChatRoom(int bodyLength)
        {
            // read request body 
            if (bodyLength != 0)
            {
                FBRoomRequestBody body = (FBRoomRequestBody)Parser.Read(peer, (int)ProtocolBodyLength.FBRoomRequestBody, typeof(FBRoomRequestBody));
                string id = new string(body.Id).Split('\0')[0];// null character 
                int result = redis.CreateChatRoom(id);
                Parser.Send(peer, BitConverter.GetBytes(result));
            }
           
            /*
             * Body가 정의되어 있지 않음 . . .
             */
            // 4byte int 를 byte[]로 전송.
            // send result
           
        }

        /*
         * Handle Leave Room
         * 1) 
         */
        public void HandleLeaveRoom()
        {

        }

        /*
         * 1) 현재 FE의 이름을 가져온다.
         * 2) 해당 FE의 CHATTING LIST를 조회.
         * 3) 결과 Header + Data 전송 
         */
        public void HandleListRoom(int length)
        {

            // 1) 현재 fe의 이름 조회 
            string feName = "";

            // 2) fe의 chatting room list 조회 

            int[] chatRoomList = (int[]) redis.ListChatRoom(feName);

            // 3) create Header
            FBHeader header = new FBHeader();
            header.Length = Marshal.SizeOf(chatRoomList);
            header.Type = FBMessageType.Room_List;
            header.State = FBMessageState.SUCCESS;

            // 3) send header
            Parser.Send(peer, header);

            // 3) send data
            Parser.Send(peer, chatRoomList);
            

        }

    }
}
