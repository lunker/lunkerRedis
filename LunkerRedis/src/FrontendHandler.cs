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
            logger.Debug("FrontendHandler constructor start");
            this.peer = peer;

            // redis setup 
            redis = RedisClient.RedisInstance;
 
            // mysql  setup 
            mysql = MySQLClient.Instance;

            logger.Debug("FrontendHandler constructor finish");
        }

        public void Initialize()
        {
            IPEndPoint ep = (IPEndPoint)peer.RemoteEndPoint;

            remoteIP = ep.Address.ToString();
            remotePort = ep.Port;
           
            logger.Debug("start");
            // 1) add 서버ip:port ~ FE Name && generate FE Name 
            string feName = redis.AddFEConnectedInfo(remoteIP, remotePort);
            if (!feName.Equals(""))
                remoteName = feName;

            // 2) 현재 접속 되어 있는 서버들의 정보 저장 
            redis.AddFEList(remoteIP, remotePort);

            Console.WriteLine("[fe_handler][Initialize] finish");
        }// end method

        public void SetFEServiceInfo()
        {
            FBHeader requestHeader = new FBHeader();
            requestHeader.Type = FBMessageType.Connection_Info;
            requestHeader.State = FBMessageState.REQUEST;
            requestHeader.Length = 0;
            requestHeader.SessionId = 0;

            // send info request
            Parser.Send(peer, requestHeader);

            // read info response
            FBHeader header = (FBHeader)Parser.Read(peer, (int)ProtocolHeaderLength.FBHeader, typeof(FBHeader));


            /******************
             * 나중에 처리 
             */
            FBConnectionInfo connectionInfo = (FBConnectionInfo) Parser.Read(peer, header.Length, typeof(FBConnectionInfo));
            
          
            string ip = new string(connectionInfo.Ip).Split('\0')[0];
            int port = connectionInfo.Port;

            redis.AddFEServiceInfo(remoteName, ip, port);
        }

        /**
         * Frontend Server의 request 처리 
         */
        public void HandleRequest()
        {
            Initialize();
            SetFEServiceInfo();

            while (true)
            {
                try
                {
                    // Read Request Header 
                    FBHeader header;
                    header = (FBHeader)Parser.Read(peer, (int)ProtocolHeaderLength.FBHeader, typeof(FBHeader));

                    switch (header.type)
                    {
                        case FBMessageType.Health_Check:
                            HandleHealthCheck();
                            break;

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

                        case FBMessageType.Logout:
                            HandleLogout(header.SessionId, header.Length);
                            break;
                        case FBMessageType.Room_Create:
                            // 채팅방 생성 : 끝 
                            HandleCreateChatRoom(header.sessionId, header.Length);
                            break;
                        case FBMessageType.Room_Leave:
                            // 채팅방 나가기 : 
                            HandleLeaveRoom(header.sessionId, header.Length);
                            break;
                        case FBMessageType.Room_Join:
                            // 채팅방 입장 : 끝
                            HandleJoinRoom(header.SessionId, header.Length);
                            break;
                        case FBMessageType.Room_List:
                            // 채팅방 목록 조회 : 끝
                            HandleListRoom(header.SessionId, header.Length);
                            break;
                        case FBMessageType.Chat_Count:
                            // 채팅 건수 저장 : 끝
                            HandleChat(header.SessionId, header.Length);
                            break;
                        default:
                            HandleError();
                            break;
                    }// end switch
                }
                catch (SocketException se)
                {
                    //Console.WriteLine("[fe_handler][HandleRequest()] disconnected . . .");
                    logger.Debug("[fe_handler][HandleRequest()] disconnected");
                    peer.Close();

                    HandleClear();
                    redis = null;
                    mysql = null;

                    logger.Debug("[fe_handler] release all resources");
                    return;
                }
            }// end loop 

        }// end method 


        public void HandleHealthCheck()
        {
            logger.Debug("[fe_handler][HandleHealthCheck()] start");
            FBHeader header = (FBHeader) Parser.Read(peer, (int)ProtocolHeaderLength.FBHeader, typeof(FBHeader) );



            logger.Debug("[fe_handler][HandleHealthCheck()] finish");
            return;
        }

        public void HandleCheckID(int sessionId, int bodyLength)
        {
            logger.Debug("[fe_handler][HandleHealthCheck()] 아이디 중복 체크 시작");
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
                //Console.WriteLine("[fe_handler][HandleCheckID] result: fail");
                logger.Debug("[fe_handler][HandleHealthCheck()] 아이디 중복 아님");
                header.State = FBMessageState.SUCCESS;
            }
            else
            {
                //Console.WriteLine("[fe_handler][HandleCheckID] result: true");
                logger.Debug("[fe_handler][HandleHealthCheck()] 아이디 중복임");
                header.State = FBMessageState.FAIL;
            }
            
            Parser.Send(peer, header);
            //Console.WriteLine("[fe_handler][HandleCheckID] finish");

            logger.Debug("[fe_handler][HandleHealthCheck()] 아이디 중복 체크 종료");
            return;
        }// end method 

        /*
         * Handle CrateUser 
         * 1) insert user info 
         * 2) send result 
         */
        public void HandleCreateUser(int sessionId, int bodyLength)
        {
            //Console.WriteLine("[fe_handler][HandleCreateUser] finish");
            logger.Debug("[fe_handler][HandleCreateUser()] 회원가입 시작");

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
                //Console.WriteLine("[fe_handler][HandleCreateUser] result: true");
                logger.Debug("[fe_handler][HandleCreateUser()] 회원가입 성공");

                header.State = FBMessageState.SUCCESS;
            }
            else
            {
                //Console.WriteLine("[fe_handler][HandleCreateUser] result: fail");
                logger.Debug("[fe_handler][HandleCreateUser()] 회원가입 실패");
                header.State = FBMessageState.FAIL;
            }
            //Console.WriteLine("[fe_handler][HandleCreateUser] finish");
            Parser.Send(peer, header);
            logger.Debug("[fe_handler][HandleCreateUser()] 회원가입 시작");
            return; 
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
            //Console.WriteLine("[fe_handler][HandleLogin()] start");
            logger.Debug("[fe_handler][HandleLogin()] 로그인 시작");
            // read body
            FBLoginRequestBody body = (FBLoginRequestBody)Parser.Read(peer, bodyLength, typeof(FBLoginRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 
            FBHeader responseHeader = new FBHeader();

            if (id.Equals(""))
            {
                // 유효하지 않은 아이디 입력 
                responseHeader.Type = FBMessageType.Login;
                responseHeader.Length = 0;
                responseHeader.SessionId = sessionId;
                responseHeader.State = FBMessageState.FAIL;

                Parser.Send(peer, responseHeader);
                logger.Debug("[fe_handler][HandleLogin()] 유효하지 않은 아이디");
                logger.Debug("[fe_handler][HandleLogin()] 로그인 종료");
                return;
            }
            // 1) db에서 사용자 정보 확인 
            User result = mysql.SelectUserInfo(id);

            responseHeader.Type = FBMessageType.Login;
            responseHeader.Length = 0;
            responseHeader.SessionId = sessionId;

            // 로그인 성공 
            if (!result.Equals(null) && result.Password.Equals(password))
            {
                responseHeader.State = FBMessageState.SUCCESS;

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
                responseHeader.Length = Marshal.SizeOf(response);

                Parser.Send(peer, responseHeader);
                Parser.Send(peer, response);
                logger.Debug("[fe_handler][HandleLogin()] 로그인 성공");

            }
            else
            {
                responseHeader.State = FBMessageState.FAIL;

                // send result : header
                Parser.Send(peer, responseHeader);
                logger.Debug("[fe_handler][HandleLogin()] 로그인 실패");
            }
            logger.Debug("[fe_handler][HandleLogin()] 로그인 종료");
            return;
        }

        /*
         * Logout 
         * 1) 사용자 id -> num id 조회 
         * 2) feName:login 에서 off 
         * 
         */
        public void HandleLogout(int sessionId, int bodyLength)
        {
            //logger.Debug("[fe_handler][HandleLogout() start");
            logger.Debug("[fe_handler][HandleLogout()] 로그아웃 시작");
            FBLoginRequestBody body = (FBLoginRequestBody)Parser.Read(peer, bodyLength, typeof(FBLoginRequestBody));
            string id = new string(body.Id).Split('\0')[0];// null character 

            int userNumId = redis.GetUserNumIdCache(id);

            FBHeader responseHeader = new FBHeader();
            responseHeader.Type = FBMessageType.Logout;
            responseHeader.Length = 0;
            responseHeader.SessionId = sessionId;

            if (redis.SetUserLogin(remoteName, userNumId, MyConst.LOGOUT))
            {
                logger.Debug("[fe_handler][HandleLogout() logout success");
                responseHeader.State = FBMessageState.SUCCESS;
            }
            else
            {
                logger.Debug("[fe_handler][HandleLogout() logout fail");
                responseHeader.State = FBMessageState.FAIL;
            }
                

            Parser.Send(peer, responseHeader);

            logger.Debug("[fe_handler][HandleLogout()] 로그아웃 종료");
            return;
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
                logger.Debug("[fe_handler][HandleCreateChatRoom() 채팅방 생성 시작");


                FBRoomRequestBody body = (FBRoomRequestBody)Parser.Read(peer, bodyLength, typeof(FBRoomRequestBody));
                string id = new string(body.Id).Split('\0')[0];// null character 
                int result = redis.CreateChatRoom(remoteName,id);
                Console.WriteLine("[fe_handler][HandleCreateChatRoom()] 생성된 채팅방 번호 : " + result);
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
            }

            logger.Debug("[fe_handler][HandleCreateChatRoom() 채팅방 생성종료");
            return;


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
            //Console.WriteLine("[fe_handler][HandleLeaveRoom()] start");

            logger.Debug("[fe_handler][HandleLeaveRoom() 채팅방 나가기 시작");

            // 1) 채팅방에서 유저 삭제 
            FBRoomRequestBody body = (FBRoomRequestBody)Parser.Read(peer, bodyLength, typeof(FBRoomRequestBody));
            string id = new string(body.Id).Split('\0')[0];// null character 
            int roomNo = body.RoomNo;


            bool leaveResult = redis.LeaveChatRoom(remoteName, roomNo, id);

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
            //Console.WriteLine("[fe_handler][HandleLeaveRoom()] finish");
            logger.Debug("[fe_handler][HandleLeaveRoom() 채팅방 나가기 시작");

        }// end method 

        /*
         * Handle Join Room
         * 
         * 1) 입장하려는 방이 같은 FE에 있는지 확인 
         * 2-1) 같은 방이면 -> 입장 및 성공 
         * 2-2) 같은 방이 아니면 실패 
         * 
         */
        public void HandleJoinRoom(int sessionId, int bodyLength)
        {
            Console.WriteLine("[fe_handler][HandleJoinRoom] start");
            FBRoomRequestBody body = (FBRoomRequestBody)Parser.Read(peer, bodyLength, typeof(FBRoomRequestBody));
            string id = new string(body.Id).Split('\0')[0];// null character 

            // 1) 같은 서버에 존재하는지 확인 
            FBHeader responseHeader = new FBHeader();
            responseHeader.Type = FBMessageType.Room_Join;
            responseHeader.SessionId = sessionId;
            // 2-1) 채팅방이 같은 서버에 존재.
            // 입장 
            if (redis.HasChatRoom(remoteName, body.RoomNo))
            {
                logger.Debug("[fe_handler][HandleJoinRoom] 같은 서버에 채팅방이 존재하여 입장!");
                redis.AddUserChatRoom(remoteName, body.RoomNo, id);
                redis.IncChatRoomCount(remoteName, body.RoomNo);
<<<<<<< HEAD

                responseHeader.State = FBMessageState.SUCCESS;
=======
  
                header.State = FBMessageState.SUCCESS;
>>>>>>> 33edd59f8de458cd784f43a2f0c119bb99734432

                
                responseHeader.Length = BitConverter.GetBytes(body.RoomNo).Length;
                Parser.Send(peer, responseHeader);
                Parser.Send(peer, BitConverter.GetBytes(body.RoomNo));
            }
            else
            {
                /*****
                 * 
                 *여 
                 * 기 
                 * 문
                 * 제 
                 * 임 
                 * ㅠ
                 * ㅠ
                 * ㅠ
<<<<<<< HEAD
                 *  
                 */
                logger.Debug("[fe_handler][HandleJoinRoom] 다른 서버에 채팅방이 존재함");
=======
                 * ㅠ
                 * ㅠ
                 *  
                 */
>>>>>>> 33edd59f8de458cd784f43a2f0c119bb99734432
                // 2-2) 채팅방이 다른 서버에 존재,
                // 다른 FE의 정보를 넘겨준다. 
                responseHeader.State = FBMessageState.FAIL;

                // feNameList : ip:port string이 넘어옴.
                string[] feIpPortList = (string[])redis.GetFEIpPortList();

                CFJoinFailBody responseBody = new CFJoinFailBody();
                

                // 해당 채팅방이 존재하는 FE의 정보 검색 
                foreach(string feIpPort in feIpPortList)
                {
                    string feName = redis.GetFEName(feIpPort);

                    // 해당 FE의 IP, PORT 전송 
                    if(redis.HasChatRoom(feName, body.RoomNo))
                    {
                        // get ce~fe service url 
                        /*
                         */
                        //FEInfo info = redis.GetFEInfo(feName);
                        FEInfo info = (FEInfo) redis.GetFEServiceInfo(feName);

                        responseBody.Ip = info.Ip.ToCharArray();
                        responseBody.Port = info.Port;
                        break;
                    }
<<<<<<< HEAD
                }// end loop
             

                responseHeader.Length = Marshal.SizeOf(responseBody);
                
                Parser.Send(peer, responseHeader);
                Parser.Send(peer, responseBody);
=======
                }

                //??????????????????
                //???????????????????????
>>>>>>> 33edd59f8de458cd784f43a2f0c119bb99734432

            }// end if 

            Console.WriteLine("[fe_handler][HandleJoinRoom] finish");
        }// end method

        /*
         * 1) 모든 FE의 CHATTING LIST를 가져와야 함. 
         * 2) 해당 FE의 CHATTING LIST를 조회.
         * 3) 결과 Header + Data 전송 
         */
        public void HandleListRoom(int sessionId, int bodyLength)
        {
            //Console.WriteLine("[fe_handler][HandleListRoom] start");
            logger.Debug("[fe_handler][HandleListRoom] start");
            // 1) 모든 FE의 이름을 가져와야 함. 
            //string feName = redis.GetFEName(remoteIP);
            string[] feIpPortList = (string[]) redis.GetFEIpPortList();


            /**
             * fe2가 계속 들어가있다.. 접속한 횟수만큼.. ㅠㅠㅠㅠ 
             */
            // 2) fe의 chatting room list 조회 
            int[] chatRoomList = null;
            foreach (string feIpPort in feIpPortList)
            {
                string feName = redis.GetFEName(feIpPort);
                if (chatRoomList != null)
                    chatRoomList.Concat((int[])redis.GetFEChattingRoomList(feName));
                else
                    chatRoomList = (int[])redis.GetFEChattingRoomList(feName);
            }
            
            // 3) create Header
            FBHeader header = new FBHeader();
            header.SessionId = sessionId;
            // generate body data
            byte[] data = chatRoomList.SelectMany(BitConverter.GetBytes).ToArray();

            if(data.Length != 0)
            {
                //Console.WriteLine("[fe_handler][HandleListRoom] data size != 0");
                logger.Debug("[fe_handler][HandleListRoom] case => data size != 0 ");
                foreach (int roomNo in chatRoomList)
                {
                    logger.Debug("[fe_handler][HandleListRoom] current room number : " + roomNo);
                }
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
                //Console.WriteLine("[fe_handler][HandleListRoom] data size ==0");
                logger.Debug("[fe_handler][HandleListRoom] case => data size == 0");
                header.Length = 0;
                header.Type = FBMessageType.Room_List;
                header.State = FBMessageState.SUCCESS;

                // 3) send header
                Parser.Send(peer, header);
            }
            //Console.WriteLine("[fe_handler][HandleListRoom] finish");
            logger.Debug("[fe_handler][HandleListRoom] finish");
        }

        /*
         * 1) add user chat count 
         */
        public void HandleChat(int sessionId, int bodyLength)
        {
            // data: user id
            //Console.WriteLine("[fe_handler][HandleChat] start");
            logger.Debug("[fe_handler][HandleChat()] start");
            FBChatRequestBody body = (FBChatRequestBody) Parser.Read(peer, bodyLength, typeof(FBChatRequestBody));

            //string key = "chatting:ranking";
            string id = new string(body.Id).Split('\0')[0];// null character 

            //redis.Get
            bool isDummy = redis.GetUserType(id);
            if (isDummy)
                return;
            else
                redis.AddChat(id);


            //Console.WriteLine("[fe_handler][HandleChat] finish");
            logger.Debug("[fe_handler][HandleChat()] start");
        }

        /*
         * Clear FE Information From redis 
         */
        public void HandleClear()
        {
            logger.Debug("[fe_handler][HandleClear()] start");

            // 1) delete 서버ip:port ~ FE Name && generate FE Name 
            redis.DelFEInfo(remoteIP, remotePort);

            // 1-1) fe service info delete
            redis.DelFEServiceInfo(remoteName);

            // 2)이하 전부 feName으로 제거! 
            // 2) fe:list에서 제거 
            // parameter : ip:port 
            redis.DelFEList(remoteIP, remotePort);

            // 3) key fe:login 삭제 
            // FE에 접속해 있는 유저들의 로그인 정보 삭제
            redis.DelUserLoginKey(remoteName);

            // 4) key fe:chattingroomlist 삭제 
            redis.DelFEChattingRoomListKey(remoteName);

            // 5-*) get room# in 삭제될 fe
            int[] roomNoList = (int[]) redis.GetFEChattingRoomList(remoteName);
            foreach(int roomNo in roomNoList)
            {
                // 5) key fe:room#:count 삭제 
                redis.DelChattingRoomCountKey(remoteName, roomNo);

                // 6) key fe:room#user 삭제 
                redis.DelUserChatRoomKey(remoteName, roomNo);
            }
            logger.Debug("[fe_handler][HandleClear()] finish");
        }

        public void HandleError()
        {
            
        }

    }
}
