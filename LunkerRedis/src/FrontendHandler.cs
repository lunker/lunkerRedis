using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using log4net;

using LunkerRedis.src.Utils;
using LunkerRedis.src.Common;
using LunkerRedis.src.Frame;

using LunkerRedis.src.Frame.FE_BE;

namespace LunkerRedis.src
{
    class FrontendHandler
    {
        private Socket peer = null;
        private RedisClient redis = null;
        private MySQLClient mysql = null;
        private string remoteIP = "";
        private int remotePort = 0;
        private string remoteName = "";

        private ILog logger = FileLogger.GetLoggerInstance();

        public FrontendHandler() { }

        public FrontendHandler(Socket peer)
        {
            logger.Debug("start");
            this.peer = peer;

            // redis setup 
            redis = new RedisClient();
            redis.Connect();
        
            // mysql  setup 
            mysql = new MySQLClient();
            mysql.Connect();

            IPEndPoint ep = (IPEndPoint) peer.RemoteEndPoint;

            remoteIP = ep.Address.ToString();
            remotePort = ep.Port;
            logger.Debug("finish");
        }


        public void Initialize()
        {
            //Console.WriteLine("[fe_handler][Initialize] start");
            logger.Debug("start");
            // 1) add 서버ip:port ~ FE Name && generate FE Name 
            string feName = redis.AddFEInfo(remoteIP, remotePort);
            if (!feName.Equals(""))
                remoteName = feName;

            // 2) add to fe:list
            redis.AddFEList(remoteIP, remotePort);

            // set remote's FE Name 
            remoteName = redis.GetFEName(remoteIP);

            Console.WriteLine("[fe_handler][Initialize] finish");
        }// end method

        /**
         * Frontend Server의 request 처리 
         */
        public void HandleRequest()
        {
            Initialize();
            while (true)
            {
                try
                {
                    // Read Request Header 
                    FBHeader header;
                    header = (FBHeader)Parser.Read(peer, (int)ProtocolHeaderLength.FBHeader, typeof(FBHeader));

                    switch (header.type)
                    {
                        case FBMessageType.Id_Dup:
                            // 아이디 중복 확인 요청 : 끝 
                            HandleCheckID(header.sessionId, header.Length);
                            break;
                        case FBMessageType.Signup:
                            // 회원가입 요청 : 끝 
                            HandleCreateUser(header.sessionId, header.Length);
                            break;
                        case FBMessageType.Login:
                            // 로그인 요청 : 끝 
                            HandleLogin(header.sessionId, header.Length);
                            break;
                        case FBMessageType.Room_Create:
                            // 채팅방 생성 : 끝 
                            HandleCreateChatRoom(header.sessionId, header.Length);
                            break;
                        case FBMessageType.Room_Leave:
                            // 채팅방 나가기 : 
                            HandleLeaveRoom(header.sessionId ,header.Length);
                            break;
                        case FBMessageType.Room_Join:
                            // 채팅방 입장 : 끝
                            HandleJoinRoom(header.Length);
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
                    }
                }
                catch (SocketException se)
                {
                    Console.WriteLine("[fe_handler] disconnected . . .");
                    peer.Close();
                    redis = null;
                    mysql = null;

                    return;
                }
            }//end loop
        }// end method 

        public void HandleCheckID(int sessionId, int bodyLength)
        {
            FBLoginRequestBody body = (FBLoginRequestBody)Parser.Read(peer, bodyLength, typeof(FBLoginRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 

            // *return true : 중복
            //*false : 중복x
            bool result = mysql.CheckIdDup(id);

            FBHeader header = new FBHeader();
            header.Type = FBMessageType.Id_Dup;
            header.Length = 0;
            header.sessionId = sessionId;

            if (!result)
            {
                Console.WriteLine("[fe_handler][HandleCheckID] result: fail");
                header.State = FBMessageState.SUCCESS;
            }
            else
            {
                Console.WriteLine("[fe_handler][HandleCheckID] result: true");
                header.State = FBMessageState.FAIL;
            }
            
            Parser.Send(peer, header);
            Console.WriteLine("[fe_handler][HandleCheckID] finish");
        }// end method 

        /*
         * Handle CrateUser 
         * 1) insert user info 
         * 2) send result 
         */
        public void HandleCreateUser(int sessionId, int bodyLength)
        {
            Console.WriteLine("[fe_handler][HandleCreateUser] finish");
            FBSignupRequestBody body = (FBSignupRequestBody)Parser.Read(peer, bodyLength, typeof(FBSignupRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 
            bool isDummy = body.IsDummy;
            
            // 1) insert user info 
            bool result =  mysql.CreateUser(id, password, isDummy);

            FBHeader header = new FBHeader();
            header.Type = FBMessageType.Signup;
            header.sessionId = sessionId;
            header.Length = 0;

            // 2) send result 
            // 회원가입성공 
            if (result)
            {
                Console.WriteLine("[fe_handler][HandleCreateUser] result: true");
                header.State = FBMessageState.SUCCESS;
            }
            else
            {
                Console.WriteLine("[fe_handler][HandleCreateUser] result: fail");
                header.State = FBMessageState.FAIL;
            }
            Console.WriteLine("[fe_handler][HandleCreateUser] finish");
            Parser.Send(peer, header);
        }
        
        /*
         * Handle Login 
         * 1) DB를 통해서 사용자 정보 확인 
         * 2) user의 정보 cache 
         * 3) 로그인 여부 저장.
         * 4) 더미 여부 저장 
         */ 
        public void HandleLogin(int sessionId, int bodyLength)
        {
            Console.WriteLine("[fe_handler][HandleLogin()] start");
            // read body
            FBLoginRequestBody body = (FBLoginRequestBody)Parser.Read(peer, bodyLength, typeof(FBLoginRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 

            // 1) db에서 사용자 정보 확인 
            User result = mysql.SelectUserInfo(id);

            FBHeader header = new FBHeader();
            header.Type = FBMessageType.Login;
            header.Length = 0;
            header.SessionId = sessionId;

            // 로그인 성공 
            if (!result.Equals(null) && result.Password.Equals(password))
            {
                header.State = FBMessageState.SUCCESS;

                // 2) cache user info 
                redis.AddUserNumIdCache(result.Id, result.NumId);

                // 3) 로그인 여부 저장
                redis.SetUserLogin(remoteName, result.NumId, MyConst.LOGINED);


                // 4) set dummy offset
                if (result.IsDummy)
                    redis.SetUserType(id, MyConst.Dummy);
                else
                    redis.SetUserType(id, MyConst.User);

                FBLoginResponseBody response = new FBLoginResponseBody();
                response.Id = body.Id;
                header.Length = Marshal.SizeOf(response);

                Parser.Send(peer, header);
                Parser.Send(peer, response);
            }
            else
            {
                header.State = FBMessageState.FAIL;

                // send result : header
                Parser.Send(peer, header);
            }
            Console.WriteLine("[fe_handler][HandleLogin()] finish");
        }

        /*
         * Logout 
         */
        public void HandleLogout()
        {

        }

        /*
         * 
         * Refactoring 대상 
         * Handle Create Chat room 
         * 
         */
        public void HandleCreateChatRoom(int sessionId, int bodyLength)
        {
            // read request body 
            if (bodyLength != 0)
            {
                Console.WriteLine("[fe_handler][HandleCreateChatRoom()] start");

                FBRoomRequestBody body = (FBRoomRequestBody)Parser.Read(peer, bodyLength, typeof(FBRoomRequestBody));
                string id = new string(body.Id).Split('\0')[0];// null character 
                int result = redis.CreateChatRoom(id);
                Console.WriteLine("[fe_handler][HandleCreateChatRoom()] created room No : " + result);
                // header
                FBHeader header = new FBHeader();
                header.Type = FBMessageType.Room_Create;
                header.SessionId = sessionId;
                header.State = FBMessageState.SUCCESS;
                //header.Length = Marshal.SizeOf(BitConverter.GetBytes(result));
                header.Length = BitConverter.GetBytes(result).Length;

                // body
                Parser.Send(peer, header);
                Parser.Send(peer, BitConverter.GetBytes(result));
                Console.WriteLine("[fe_handler][HandleCreateChatRoom()] finish");
            }
           
            /*
             * Body가 정의되어 있지 않음 . . .
             */
            // 4byte int 를 byte[]로 전송.
            // send result
        }

        /*
         * Handle Leave Room
         * 1) 채팅방에서 유저 삭제 
         * 2) 채팅방의 COUNT 감소 
         * 
         * 3) If) count == 0 이면 방 삭제 
         */
        public void HandleLeaveRoom(int sessionId, int bodyLength)
        {
            Console.WriteLine("[fe_handler][HandleLeaveRoom()] start");

            // 1) 채팅방에서 유저 삭제 
            FBRoomRequestBody body = (FBRoomRequestBody)Parser.Read(peer, bodyLength, typeof(FBRoomRequestBody));
            string id = new string(body.Id).Split('\0')[0];// null character 
            int roomNo = body.RoomNo;


            bool leaveResult = redis.LeaveChatRoom(remoteName, id, roomNo);

            // 2) 채팅방의 COUNT 감소 
            int decResult = redis.DecChatRoomCount(id,roomNo);

            if(leaveResult && decResult == 0)
            {
                // 방삭제 
                Console.WriteLine("[fe_handler][HandleLeaveRoom()] leave room && delete room ");
            }
            else if(leaveResult && decResult != 0)
            {
                // send result 
                Console.WriteLine("[fe_handler][HandleLeaveRoom()] leave room && not delete room ");
            }
            Console.WriteLine("[fe_handler][HandleLeaveRoom()] finish");

        }// end method 

        /*
         * Handle Join Room
         * 
         * 1) 입장하려는 방이 같은 FE에 있는지 확인 
         * 2-1) 같은 방이면 -> 입장 및 성공 
         * 2-2) 같은 방이 아니면 실패 
         * 
         */
        public void HandleJoinRoom(int bodyLength)
        {
            Console.WriteLine("[fe_handler][HandleJoinRoom] start");
            FBRoomRequestBody body = (FBRoomRequestBody)Parser.Read(peer, bodyLength, typeof(FBRoomRequestBody));
            string id = new string(body.Id).Split('\0')[0];// null character 

            // 1) 같은 서버에 존재하는지 확인 
            FBHeader header = new FBHeader();
            header.Type = FBMessageType.Room_Join;

            // 2-1) 채팅방이 같은 서버에 존재.
            // 입장 
            if (redis.HasChatRoom(remoteName, body.RoomNo))
            {
                header.State = FBMessageState.SUCCESS;

                //header.Length = Marshal.SizeOf(BitConverter.GetBytes(body.RoomNo));
                header.Length = BitConverter.GetBytes(body.RoomNo).Length;
                Parser.Send(peer, header);
                Parser.Send(peer, BitConverter.GetBytes(body.RoomNo));
            }
            else
            {
                // 2-2) 채팅방이 다른 서버에 존재,
                // 다른 FE의 정보를 넘겨준다. 
                header.State = FBMessageState.FAIL;

                string[] feNameList = (string[])redis.GetFEList();
         
                // 해당 채팅방이 존재하는 FE의 정보 검색 
                foreach(string feName in feNameList)
                {
                    // 해당 FE의 IP, PORT 전송 
                    if(redis.HasChatRoom(feName, body.RoomNo))
                    {
                        FEInfo info = redis.GetFEInfo(feName);

                        byte[] ipArr = Encoding.UTF8.GetBytes(info.Ip);
                        byte[] portArr = BitConverter.GetBytes(info.Port);

                        //header.Length = Marshal.SizeOf(ipArr) + Marshal.SizeOf(portArr);
                        header.Length = ipArr.Length + portArr.Length;

                        byte[] data = new byte[header.Length];
                        Buffer.BlockCopy(ipArr,0,data,0,ipArr.Length);
                        Buffer.BlockCopy(portArr, 0, data, ipArr.Length, data.Length);

                        Parser.Send(peer, header);
                        Parser.Send(peer, data);

                        break;
                    }
                }
            }// end if 

            Console.WriteLine("[fe_handler][HandleJoinRoom] finish");
        }// end method

        /*
         * 1) 모든 FE의 CHATTING LIST를 가져와야 함. 
         * 2) 해당 FE의 CHATTING LIST를 조회.
         * 3) 결과 Header + Data 전송 
         */
        public void HandleListRoom(int bodyLength)
        {
            Console.WriteLine("[fe_handler][HandleListRoom] start");
            // 1) 모든 FE의 이름을 가져와야 함. 
            //string feName = redis.GetFEName(remoteIP);
            string[] feList = (string[]) redis.GetFEList();

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
            // generate body data
            byte[] data = chatRoomList.SelectMany(BitConverter.GetBytes).ToArray();

            if(data.Length != 0)
            {
                header.Length = data.Length;
                header.Type = FBMessageType.Room_List;
                header.State = FBMessageState.SUCCESS;

                // 3) send header
                Parser.Send(peer, header);

                // 3) send data
                Parser.Send(peer, data);

            }
            else
            {
                header.Length = data.Length;
                header.Type = FBMessageType.Room_List;
                header.State = FBMessageState.SUCCESS;

                // 3) send header
                Parser.Send(peer, header);
            }
            Console.WriteLine("[fe_handler][HandleListRoom] finish");
        }

        /*
         * 1) add user chat count 
         */
        public void HandleChat(int bodyLength)
        {
            // data: user id
            Console.WriteLine("[fe_handler][HandleChat] start");
            FBChatRequestBody body = (FBChatRequestBody) Parser.Read(peer, bodyLength, typeof(FBChatRequestBody));

            //string key = "chatting:ranking";
            string id = new string(body.Id).Split('\0')[0];// null character 

            //redis.Get
            bool isDummy = redis.GetUserType(id);
            if (isDummy)
                return;
            else
                redis.AddChat(id);


            Console.WriteLine("[fe_handler][HandleChat] finish");
        }

        public void HandleError()
        {
            
        }

    }
}
