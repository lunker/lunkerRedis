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
        private string remoteName = "";

        public FrontendHandler() { }

        public FrontendHandler(Socket peer)
        {
            this.peer = peer;

            IPEndPoint ep = (IPEndPoint) peer.RemoteEndPoint;
            remoteIP = ep.Address.ToString();
            remoteName = redis.GetFEName(remoteIP);
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
                        HandleCheckID(header.Length);
                        break;
                    case FBMessageType.Signup:
                        // 회원가입 요청 : 끝 
                        HandleCreateUser(header.Length);
                        break;

                    case FBMessageType.Login:
                        // 로그인 요청 : 끝 
                        HandleLogin(header.Length);
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
                        // 채팅방 목록 조회 : 끝
                        HandleListRoom(header.Length);
                        break;
                    case FBMessageType.Chat_Count:
                        // 채팅 건수 저장 : 끝 
                        HandleChat(header.Length);
                        break;
                    default:
                        HandleError();
                        break;
                }// end switch
            }//end loop
        }// end method 

        public void HandleCheckID(int bodyLength)
        {
            
        }


        /*
         * Handle CrateUser 
         * 1) insert user info 
         * 2) send result 
         */
        public void HandleCreateUser(int bodyLength)
        {
            FBSignupRequestBody body = (FBSignupRequestBody)Parser.Read(peer, (int)ProtocolBodyLength.FBSignupRequestBody, typeof(FBSignupRequestBody));
            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 

            // 1) insert user info 
            bool result =  mysql.CreateUser(id, password);

            FBHeader header = new FBHeader();
            header.Type = FBMessageType.Signup;
            header.Length = 0;

            // 2) send result 
            // 회원가입성공 
            if (result)
            {
                header.State = FBMessageState.SUCCESS;
            }
            else
            {
                header.State = FBMessageState.FAIL;
            }
            Parser.Send(peer, header);
        }
        
        /*
         * Handle Login 
         * 1) DB를 통해서 사용자 정보 확인 
         * 2) user의 정보 cache 
         * 3) 로그인 여부 저장.
         */ 
        public void HandleLogin(int bodyLength)
        {
            // read body
            FBLoginRequestBody body = (FBLoginRequestBody)Parser.Read(peer, (int)ProtocolBodyLength.FBLoginRequestBody, typeof(FBLoginRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 

            // 1) db에서 사용자 정보 확인 
            User result = mysql.SelectUserInfo(id);


            FBHeader header = new FBHeader();
            header.Type = FBMessageType.Login;
            header.Length = 0;

            // 로그인 성공 
            if (result.Password.Equals(password))
            {
                header.State = FBMessageState.SUCCESS;

                // 2) cache user info 
                redis.AddUserCache(result.Id, result.NumId);

                // 3) 로그인 여부 저장
                redis.SetUserLogin(remoteName, result.NumId, MyConst.LOGINED);
            }
            else
            {
                header.State = FBMessageState.FAIL;
            }

            // send result : header
            Parser.Send(peer, header);
        }

        /*
         * Handle Create Chat room 
         */
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
         * Handle Join Room
         */
        public void HandleJoinRoom()
        {



        }

        /*
         * 1) 모든 FE의 CHATTING LIST를 가져와야 함. 
         * 2) 해당 FE의 CHATTING LIST를 조회.
         * 3) 결과 Header + Data 전송 
         */
        public void HandleListRoom(int bodyLength)
        {

            // 1) 모든 FE의 이름을 가져와야 함. 
            //string feName = redis.GetFEName(remoteIP);
            string[] feList = (string[]) redis.GetFENameList();

            // 2) fe의 chatting room list 조회 
            int[] chatRoomList = null;
            foreach (string fe in feList)
            {
                if (chatRoomList != null)
                    chatRoomList.Concat((int[])redis.ListChatRoom(fe));
                else
                    chatRoomList = (int[])redis.ListChatRoom(fe);
            }
            
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

        /*
         * 1) add user chat count 
         */
        public void HandleChat(int bodyLength)
        {
            // data: user id, roomNo; 

            FBChatRequestBody body = (FBChatRequestBody) Parser.Read(peer, bodyLength, typeof(FBChatRequestBody));

            //string key = "chatting:ranking";
            string id = new string(body.Id).Split('\0')[0];// null character 

            redis.AddChat(id);
        }

        public void HandleError()
        {
            
        }

    }
}
