﻿using System;
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
using System.Timers;
using System.Threading;

namespace LunkerRedis.src
{
    class FrontendHandler
    {
        private Socket peer = null; // frontend server socket
        private RedisClient redis = null; 
        private MySQLClient mysql = null;

        private string remoteIP = ""; // frontend server IP
        private int remotePort = 0; // frontend server socket port
        private string remoteName = ""; // frontend server nickname
        private int remoteServicePort = 0; // frontend server service port
        
        private int healthCheckedCount = 0; // healcheck count 
        private bool threadState = MyConst.Run; // thread state
        private System.Timers.Timer healthCheckTimer = null;

        private ILog logger = FileLogger.GetLoggerInstance();

        public FrontendHandler() { }

        public FrontendHandler(Socket peer)
        {
            logger.Debug("FrontendHandler constructor start");
            this.peer = peer;

            // redis setup 
            redis = RedisClientPool.GetInstance();

            // mysql  setup 
            mysql = MySQLClientPool.GetInstance();

            logger.Debug("FrontendHandler constructor finish");
        }

        /// <summary>
        /// Get FE Connected Information
        /// </summary>
        public void Initialize()
        {
            IPEndPoint ep = (IPEndPoint)peer.RemoteEndPoint;

            remoteIP = ep.Address.ToString();
            remotePort = ep.Port;

            // 1) add 서버ip:port ~ FE Name && generate FE Name 
            string feName = redis.AddFEConnectedInfo(remoteIP, remotePort);
            if (!feName.Equals(""))
                remoteName = feName;

            // 2) 현재 접속 되어 있는 서버들의 정보 저장 
            redis.AddFEList(remoteIP, remotePort);

        }// end method

        public void SetFEServiceInfo()
        {
            FBHeader requestHeader = new FBHeader();
            requestHeader.Type = FBMessageType.Connection_Info;
            requestHeader.State = FBMessageState.REQUEST;
            requestHeader.Length = 0;
            requestHeader.SessionId = 0;

            // send info request
            NetworkManager.Send(peer, requestHeader);

            // read info response
            FBHeader header = (FBHeader)NetworkManager.Read(peer, (int)ProtocolHeaderLength.FBHeader, typeof(FBHeader));

  
            FBConnectionInfo connectionInfo = (FBConnectionInfo) NetworkManager.Read(peer, header.Length, typeof(FBConnectionInfo));
          
            string ip = new string(connectionInfo.Ip).Split('\0')[0];
            int port = connectionInfo.Port;

            redis.AddFEServiceInfo(remoteName, ip, port);

            remoteServicePort = port;
            logger.Debug($"[fe_handler][{remoteName}] real fe info {ip} : {port}");
        }
         
        /// <summary>
        /// Stop Thread
        /// </summary>
        public void HandleStopThread()
        {
            threadState = MyConst.Exit;
        }

        /// <summary>
        /// Handle Frontend Request 
        /// </summary>
        public void HandleRequest()
        {
            try
            {
                StartHealthCheckTimer(); // start HealthCheckTimer
                Initialize(); // Initialize FrontEnd Handle
                SetFEServiceInfo(); // Get Connected FE Info
            }
            catch (SocketException se)
            {
                peer.Close();

                RedisClientPool.ReleaseObject(redis);
                MySQLClientPool.ReleaseObject(mysql);

                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}] release all resources");
                return;
            }

            try
            {
                while (threadState)
                {
                    // Read Request Header 
                    FBHeader header;
                    header = (FBHeader)NetworkManager.Read(peer, (int)ProtocolHeaderLength.FBHeader, typeof(FBHeader));

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

                }// end loop 
            }
            catch (SocketException se)
            {
                logger.Debug($"[fe_handler][HandleRequest()][{remoteName}][{remoteServicePort}] disconnected");
                healthCheckTimer.Stop();
                peer.Close();

                threadState = MyConst.Exit;

                logger.Debug($"[fe_handler][HandleRequest()][{remoteName}][{remoteServicePort}] release all resources");
                return;
            }
            finally
            {
                HandleClear();

                RedisClientPool.ReleaseObject(redis);
                MySQLClientPool.ReleaseObject(mysql);
                logger.Debug($"[fe_handler][HandleRequest()][{remoteName}][{remoteServicePort}] stop thread");
            }
          
            return;
        }// end method 

        /// <summary>
        /// Start Health CheckTimer 
        /// </summary>
        public void StartHealthCheckTimer()
        {
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}] Health Check Timer Start");
            healthCheckTimer = new System.Timers.Timer();
            healthCheckTimer.Interval = 60 * 1000; // 5초
            healthCheckTimer.Elapsed += new ElapsedEventHandler(ReceiveHealthCheck);
            healthCheckTimer.Start();
        }

        /// <summary>
        /// Recevie health Check 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReceiveHealthCheck(object sender, ElapsedEventArgs e)
        {
            //Interlocked.Increment(ref healthCheckedCount);
            mysql.Ping();
            healthCheckedCount++;

            if(healthCheckedCount == 3)
            {
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}] Health Check Timer Timeout. Close Connection");
                threadState = MyConst.Exit;
                // peer die
                if (peer != null)
                {
                    peer.Close();
                }
            }
            return;
        }

        /// <summary>
        ///  Handle Health Check message
        /// </summary>
        public void HandleHealthCheck()
        {
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleHealthCheck()] start");

            FBHeader header = new FBHeader();
            header.Length = 0;
            header.Type = FBMessageType.Health_Check;
            header.State = FBMessageState.SUCCESS;

            NetworkManager.Send(peer, header);

            healthCheckedCount = 0;
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleHealthCheck()] finish");
            return;
        }

        /// <summary>
        /// Handle Request Check id duplicate
        /// </summary>
        /// <param name="sessionId">client sessionid</param>
        /// <param name="bodyLength">body message Length</param>
        public void HandleCheckID(int sessionId, int bodyLength)
        {
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCheckID()] 아이디 중복 체크 시작");
            FBLoginRequestBody body = (FBLoginRequestBody)NetworkManager.Read(peer, bodyLength, typeof(FBLoginRequestBody));

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
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCheckID()] 아이디 중복 아님");
                header.State = FBMessageState.SUCCESS;
            }
            else
            {
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCheckID()] 아이디 중복임");
                header.State = FBMessageState.FAIL;
            }
            
            NetworkManager.Send(peer, header);

            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCheckID()] 아이디 중복 체크 종료");
            return;
        }// end method 

        /*
         * Handle CrateUser 
         * 1) insert user info 
         * 2) send result 
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionId">client sessionid</param>
        /// <param name="bodyLength">body message Length</param>
        public void HandleCreateUser(int sessionId, int bodyLength)
        {
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCreateUser()] 회원가입 시작");

            FBSignupRequestBody body = (FBSignupRequestBody)NetworkManager.Read(peer, bodyLength, typeof(FBSignupRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 
            bool isDummy = body.IsDummy;

            // 0) 가입되어 있는지 확인 . .  .
            User user =  mysql.SelectUserInfo(id);

            FBHeader header = new FBHeader();
            header.Type = FBMessageType.Signup;
            header.sessionId = sessionId;
            header.Length = 0;


            // 해당 아이디로 정보가 있따.
            if (user != null)
            {
                header.State = FBMessageState.FAIL;

            }
            else
            {
                // 1) insert user info 
                bool result = mysql.CreateUser(id, password, isDummy);
                // 2) send result 
                // 회원가입성공 
                if (result)
                {
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCreateUser()] 회원가입 성공");

                    header.State = FBMessageState.SUCCESS;
                }
                else
                {
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCreateUser()] 회원가입 실패");
                    header.State = FBMessageState.FAIL;
                }
              
            }
            NetworkManager.Send(peer, header);
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCreateUser()] 회원가입 종료");

            return; 
        }


        /// <summary>
        /// Handle Login
        /// </summary>
        /// <param name="sessionId">client sessionid</param>
        /// <param name="bodyLength">body message Length</param>
        public void HandleLogin(int sessionId, int bodyLength)
        {
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogin()] 로그인 시작");
            // read body
            FBLoginRequestBody body = (FBLoginRequestBody)NetworkManager.Read(peer, bodyLength, typeof(FBLoginRequestBody));

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

                NetworkManager.Send(peer, responseHeader);
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogin()] 유효하지 않은 아이디");
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogin()] 로그인 종료");
                return;
            }
            // 1) db에서 사용자 정보 확인 
            User result = mysql.SelectUserInfo(id);

            responseHeader.Type = FBMessageType.Login;
            responseHeader.Length = 0;
            responseHeader.SessionId = sessionId;

            // 다른 서버에 유저가 접속되어 있는지 확인.

            // 정보 일치 
            if (result != null && result.Password.Equals(password))
            {
                responseHeader.State = FBMessageState.SUCCESS;

                // 2) cache user info 
                redis.AddUserNumIdCache(result.Id, result.NumId);
        
                string[] ipPortList = (string[])redis.GetFEIpPortList();
                foreach (string ipPort in ipPortList)
                {
                    string feName = redis.GetFEName(ipPort);
                    if(redis.GetUserLogin(feName, result.NumId))
                    {
                       // login Fail
                       // 이미 어딘가에 접속해 있음.
                        responseHeader.State = FBMessageState.FAIL;

                        // send result : header
                        NetworkManager.Send(peer, responseHeader);
                        logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogin()] 다른 서버에 해당 유저 들어가있음.");
                        logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogin()] 로그인 실패");
                        return;
                    }
                }// end loop

                // 3) 로그인 여부 저장
                redis.SetUserLogin(remoteName, result.NumId, MyConst.LOGINED);
                // 저장 여부 확인 
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}] login bit set result : " + redis.GetUserLogin(remoteName, result.NumId));


                // 4) set dummy offset
                if (result.IsDummy)
                    redis.SetUserType(id, MyConst.Dummy);
                else
                    redis.SetUserType(id, MyConst.User);

                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}] 유저의 더미 여부 : {result.IsDummy}");


                FBLoginResponseBody response = new FBLoginResponseBody();
                response.Id = body.Id;
                responseHeader.Length = Marshal.SizeOf(response);

                NetworkManager.Send(peer, responseHeader);
                NetworkManager.Send(peer, response);
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogin()] 로그인 성공");

            }
            else
            {
                responseHeader.State = FBMessageState.FAIL;

                // send result : header
                NetworkManager.Send(peer, responseHeader);
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogin()] 로그인 실패");
            }
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogin()] 로그인 종료");
            return;
        }

        /*
         * Logout 
         * 1) 사용자 id -> num id 조회 
         * 2) feName:login 에서 off 
         * 
         */
         /// <summary>
         /// Handle Logout Request 
         /// </summary>
         /// <param name="sessionId">Client Session Id</param>
         /// <param name="bodyLength">Body Length</param>
        public void HandleLogout(int sessionId, int bodyLength)
        {
            //logger.Debug("[fe_handler][HandleLogout() start");
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogout()] 로그아웃 시작");

            FBLoginRequestBody body = (FBLoginRequestBody)NetworkManager.Read(peer, bodyLength, typeof(FBLoginRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 
            int userNumId = redis.GetUserNumIdCache(id);

            FBHeader responseHeader = new FBHeader();
            responseHeader.Type = FBMessageType.Logout;
            responseHeader.Length = 0;
            responseHeader.SessionId = sessionId;

            //
            //  현재 서버에서 사용자가 있는 방 찾기 
            // 방에 접속해 있는 유저가 강제 종료 및 끝내기로 로그아웃을 할 경우,
            // 기존에 접속해 있는 방에 대한 정보를 찾아서 나가기 시도를 한다.

            int enteredRoomNo = redis.GetUserEnteredRoomNo(remoteName, id);

            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogout()] 나가려는 사용자가 들어 있는 방 : ");
            if (enteredRoomNo != -1)
            {
                redis.LeaveChatRoom(remoteName, enteredRoomNo, id);
                if (redis.DecChatRoomCount(remoteName, enteredRoomNo) == 0)
                {
                    //방삭제 

                    redis.DelChattingRoom(remoteName, enteredRoomNo);
                    redis.DelChattingRoomCountKey(remoteName, enteredRoomNo);
                    redis.DelUserChatRoomKey(remoteName, enteredRoomNo);
                }
            }
            if(userNumId != -1)
            {
                if (redis.SetUserLogin(remoteName, userNumId, MyConst.LOGOUT))
                {
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogout() result true : ");
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogout() logout success");
                    responseHeader.State = FBMessageState.SUCCESS;
                }
                else
                {
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogout() result fail : ");
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogout() logout fail");
                    responseHeader.State = FBMessageState.FAIL;
                }

                NetworkManager.Send(peer, responseHeader);

                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLogout()] 로그아웃 종료");
            }
            return;
        }// end method

        /// <summary>
        /// Handle Create Chatting Room Request
        /// </summary>
        /// <param name="sessionId">Client Session Id</param>
        /// <param name="bodyLength">Body Length</param>
        public void HandleCreateChatRoom(int sessionId, int bodyLength)
        {
            // read request body 
            if (bodyLength != 0)
            {
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCreateChatRoom() 채팅방 생성 시작");


                FBRoomRequestBody body = (FBRoomRequestBody)NetworkManager.Read(peer, bodyLength, typeof(FBRoomRequestBody));
                string id = new string(body.Id).Split('\0')[0];// null character 
                int result = redis.CreateChatRoom(remoteName,id);
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCreateChatRoom()] 생성된 채팅방 번호 : {result}");

                // header
                FBHeader header = new FBHeader();
                header.Type = FBMessageType.Room_Create;
                header.SessionId = sessionId;
                header.State = FBMessageState.SUCCESS;
                header.Length = BitConverter.GetBytes(result).Length;

                // body
                NetworkManager.Send(peer, header);
                NetworkManager.Send(peer, BitConverter.GetBytes(result));
            }

            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleCreateChatRoom() 채팅방 생성종료");
            return;
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
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLeaveRoom() 채팅방 나가기 시작");

            // 1) 채팅방에서 유저 삭제 
            FBRoomRequestBody body = (FBRoomRequestBody)NetworkManager.Read(peer, bodyLength, typeof(FBRoomRequestBody));
            string id = new string(body.Id).Split('\0')[0];// null character 
            int roomNo = body.RoomNo;

            // 2) 채팅방의 user 나가기.
            bool leaveResult = redis.LeaveChatRoom(remoteName, roomNo, id);

            // 2) 채팅방의 COUNT 감소 
            // 감소한 이후의 결과가 온다.
            int decResult = redis.DecChatRoomCount(remoteName, roomNo);

            FBHeader responseHeader = new FBHeader();
            responseHeader.Type = FBMessageType.Room_Leave;
            responseHeader.State = FBMessageState.SUCCESS;
            responseHeader.Length = 0;
            responseHeader.SessionId = sessionId;

            NetworkManager.Send(peer, responseHeader);

            if (leaveResult && decResult == 0)
            {
                // 방삭제 
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLeaveRoom() 나가기 이후, result == 0");

                redis.DelUserChatRoomKey(remoteName, roomNo);
                redis.DelChattingRoomCountKey(remoteName, roomNo);
                redis.DelChattingRoom(remoteName, roomNo);

                // 추가적인 방삭제 완료 헤더 전송 
                responseHeader.Type = FBMessageType.Room_Delete;
                responseHeader.Length = BitConverter.GetBytes(roomNo).Length;
                NetworkManager.Send(peer, responseHeader);
                
                NetworkManager.Send(peer,BitConverter.GetBytes(roomNo));
            }
            else if(leaveResult && decResult != 0)
            {
                // send result 
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLeaveRoom() 나가기 이후, result != 0");
            }

            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleLeaveRoom() 채팅방 나가기 종료");
            return;
        }// end method 

        /// <summary>
        /// Handle Join Room
        /// 
        /// 1) 입장하려는 방이 같은 FE에 있는지 확인 
        /// 2-1) 같은 방이면 -> 입장 및 성공 
        /// 2-2) 같은 방이 아니면 실패 
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="bodyLength"></param>
        public void HandleJoinRoom(int sessionId, int bodyLength)
        {
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleJoinRoom] start");
            FBRoomRequestBody body = (FBRoomRequestBody)NetworkManager.Read(peer, bodyLength, typeof(FBRoomRequestBody));
            string id = new string(body.Id).Split('\0')[0];// null character 

            // 1) 같은 서버에 존재하는지 확인 
            FBHeader responseHeader = new FBHeader();
            responseHeader.Type = FBMessageType.Room_Join;
            responseHeader.SessionId = sessionId;

            // 2-1) 채팅방이 같은 서버에 존재.
            // 입장 
            if (redis.HasChatRoom(remoteName, body.RoomNo))
            {
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleJoinRoom] 같은 서버에 채팅방이 존재하여 입장!");
                redis.AddUserChatRoom(remoteName, body.RoomNo, id);
                redis.IncChatRoomCount(remoteName, body.RoomNo);

                responseHeader.State = FBMessageState.SUCCESS;

                responseHeader.Length = BitConverter.GetBytes(body.RoomNo).Length;
                NetworkManager.Send(peer, responseHeader);
                NetworkManager.Send(peer, BitConverter.GetBytes(body.RoomNo));

                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleJoinRoom] 채팅방 입장 종료");
                return;
            }
            else
            {
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleJoinRoom] 다른 서버에 채팅방이 존재함");

                // 2-2) 채팅방이 다른 서버에 존재,
                // 다른 FE의 정보를 넘겨준다. 
                responseHeader.State = FBMessageState.FAIL;

                // feNameList : ip:port string이 넘어옴.
                string[] feIpPortList = (string[])redis.GetFEIpPortList();

                CFJoinFailBody responseBody = new CFJoinFailBody();

                bool flag = false;

                // 해당 채팅방이 존재하는 FE의 정보 검색 
                foreach (string feIpPort in feIpPortList)
                {
                    string feName = redis.GetFEName(feIpPort);

                    // 해당 FE의 IP, PORT 전송 
                    if (redis.HasChatRoom(feName, body.RoomNo))
                    {
                        flag = true;

                        FEInfo info = (FEInfo)redis.GetFEServiceInfo(feName);
                        responseBody.Ip = new char[15];
                        //responseBody.Ip = info.Ip.ToCharArray();
                        Array.Copy(info.Ip.ToCharArray(), responseBody.Ip, info.Ip.ToCharArray().Length);
                        responseBody.Port = info.Port;
                        break;
                    }

                }// end loop

                // 다른 서버에 채팅방 존재
                if (flag)
                {
                    responseHeader.Length = Marshal.SizeOf(responseBody);

                    NetworkManager.Send(peer, responseHeader);
                    NetworkManager.Send(peer, responseBody);
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleJoinRoom] 다른 서버에 채팅방이 존재, 종료");
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleJoinRoom] 채팅방 입장 종료");
                }
                else
                {
                    // 없는 채팅방에 입장 시도 
                    responseHeader.Length = 0;
                    NetworkManager.Send(peer, responseHeader);
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleJoinRoom] 없는 채팅방에 접속시도, 종료");
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleJoinRoom] 채팅방 입장 종료");
                }
            }// end if
            return;
        }// end method

        /*
         * 1) 모든 FE의 CHATTING LIST를 가져와야 함. 
         * 2) 해당 FE의 CHATTING LIST를 조회.
         * 3) 결과 Header + Data 전송 
         */
         /// <summary>
         /// handle chatting room list request 
         /// <para>1) get all FE Name List by fFE' ip:port (string)</para>
         /// <para>2) </para>
         /// </summary>
         /// <param name="sessionId"></param>
         /// <param name="bodyLength"></param>
        public void HandleListRoom(int sessionId, int bodyLength)
        {
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleListRoom] start");
            // 1) 모든 FE의 이름을 가져와야 함. 
           
            string[] feIpPortList = (string[]) redis.GetFEIpPortList();

            // 2) fe의 chatting room list 조회 
            
            List<int> chatRoomList = new List<int>();
     
            foreach (string feIpPort in feIpPortList)
            {
                string feName = redis.GetFEName(feIpPort);

                foreach(int roomNo in (int[])redis.GetFEChattingRoomList(feName))
                {
                    chatRoomList.Add(roomNo);
                }
            }
            
            // 3) create Header
            FBHeader header = new FBHeader();
            header.SessionId = sessionId;
            // generate body data

            byte[] data = chatRoomList.SelectMany(BitConverter.GetBytes).ToArray();

            if(data.Length != 0)
            {
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleListRoom] case => data size != 0 ");
                foreach (int roomNo in chatRoomList)
                {
                    logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleListRoom] current room number : " + roomNo);
                }
                header.Length = data.Length;
                header.Type = FBMessageType.Room_List;
                header.State = FBMessageState.SUCCESS;

                // 3) send header
                NetworkManager.Send(peer, header);

                // 3) send data
                NetworkManager.Send(peer, data);
            }
            else
            {
                logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleListRoom] case => data size == 0");
                header.Length = 0;
                header.Type = FBMessageType.Room_List;
                header.State = FBMessageState.SUCCESS;

                // 3) send header
                NetworkManager.Send(peer, header);
            }
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleListRoom] finish");
            return;
        }

         /// <summary>
         /// Save Chatting 
         /// </summary>
         /// <param name="sessionId"></param>
         /// <param name="bodyLength"></param>
        public void HandleChat(int sessionId, int bodyLength)
        {
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleChat] 채팅 저장 시작 ");
            FBChatRequestBody body = (FBChatRequestBody) NetworkManager.Read(peer, bodyLength, typeof(FBChatRequestBody));

            //string key = "chatting:ranking";
            string id = new string(body.Id).Split('\0')[0];// null character 

            //redis.Get
            bool isDummy = redis.GetUserType(id);
            if (isDummy)
                return;
            else
                redis.AddChat(id);

            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleChat] 채팅 저장 끝 ");
            return;
        }

       
         /// <summary>
         /// Clear FE cached information from Redis 
         /// </summary>
        public void HandleClear()
        {
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleClear()] start");

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
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleClear()] finish");
        }

        public void HandleError()
        {
            logger.Debug($"[fe_handler][{remoteName}][{remoteServicePort}][HandleError] 정의되지 않은 메세지  ");
            return;
        }

    }
}
