using LunkerRedis.src.Common;
using LunkerRedis.src.Frame;
using LunkerRedis.src.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace LunkerRedis.src
{
    class ClientHandler
    {
        private Socket peer = null;
        private RedisClient redis = null;
        private MySQLClient mysql = null;

        private ILog logger = FileLogger.GetLoggerInstance();

        public ClientHandler() { }
        public ClientHandler(Socket handler)
        {
            this.peer = handler;
            redis = RedisClient.RedisInstance;
            mysql = MySQLClient.Instance;
        }

        /**
         * Monitoring Client의 request 처리 
         */
        public void HandleRequest()
        {
            logger.Debug("start");
            while (true)
            {
                // Read Request
                try
                {
                    CBHeader header;
                    header = (CBHeader)Parser.Read(peer, (int)ProtocolHeaderLength.CBHeader, typeof(CBHeader));

                    switch (header.type)
                    {
                        case CBMessageType.Login:
                            HandleLogin(header.Length);
                            break;

                        case CBMessageType.Total_Room_Count:
                            HandleRequestTotalRoomCount(header.Length);
                            break;
                        case CBMessageType.FE_User_Status:
                            HandleFEUserStatus(header.Length);
                            break;
                        case CBMessageType.Chat_Ranking:
                            HandleChatRanking(header.Length);
                            break;
                        default:
                            //HandleError();
                            break;
                    }
                }
                catch (SocketException se)
                {
                    Console.WriteLine("error!!!!!!!!!!!!!!!!!!!!!!");
                    peer.Close();
                    logger.Debug("close handler");
                    return;
                }
            }//end loop

        }//end method


        public void HandleLogin(int bodyLength)
        {
            //Console.WriteLine("[ce_handler][HandleLogin()] start");
            logger.Debug("[ce_handler][HandleLogin()] 로그인 시작");
            // read body
            CBLoginRequestBody body = (CBLoginRequestBody)Parser.Read(peer, bodyLength, typeof(CBLoginRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 
            CBHeader responseHeader = new CBHeader();

            if (id.Equals(""))
            {
                // 유효하지 않은 아이디 입력 
                responseHeader.Type = CBMessageType.Login;
                responseHeader.Length = 0;
                responseHeader.State = CBMessageState.FAIL;

                Parser.Send(peer, responseHeader);
                logger.Debug("[ce_handler][HandleLogin()] 유효하지 않은 아이디");
                logger.Debug("[ce_handler][HandleLogin()] 로그인 종료");
                return;
            }
            // 1) db에서 사용자 정보 확인 
            User result = mysql.SelectUserInfo(id);

            responseHeader.Type = CBMessageType.Login;
            responseHeader.Length = 0;

            // 로그인 성공 
            if (!result.Equals(null) && result.Password.Equals(password))
            {
                responseHeader.State = CBMessageState.SUCCESS;
                logger.Debug("[ce_handler][HandleLogin()] 로그인 성공");
            }
            else
            {
                responseHeader.State = CBMessageState.FAIL;
                logger.Debug("[ce_handler][HandleLogin()] 로그인 실패");
            }

            Parser.Send(peer, responseHeader);
            logger.Debug("[ce_handler][HandleLogin()] 로그인 종료");
            return;

        }

        /*
         * 전체 채팅방 수 조회 
         * 1) 
         */
        public void HandleRequestTotalRoomCount(int bodyLength)
        {
            logger.Debug("[ce_handler][HandleRequestTotalRoomCount()] 전체 채팅방 개수 조회 시작");
            string[] feList = (string[]) redis.GetFEIpPortList();
            int sum = 0;
            foreach (string fe in feList)
            {
                string feName = redis.GetFEName(fe);

                sum += redis.GetFETotalChatRoomCount(feName);
            }

            CBHeader header = new CBHeader();
            header.Type = CBMessageType.Total_Room_Count;
            header.State = CBMessageState.SUCCESS;

            byte[] data = BitConverter.GetBytes(sum);

            header.Length = data.Length;

            Parser.Send(peer, header);
            Parser.Send(peer, data);
            logger.Debug("[ce_handler][HandleRequestTotalRoomCount()] 전체 채팅방 개수 조회 종료");
            return;
        }// end method


        /*
         * FE별 사용자 수 조회 
         * 1) 전체 FE IP:PORT string 가져옴 
         * 2) 가져온 string -> fe Name 조회 
         * 3) fename:chattingroomlist를 통해서 채팅방 번호들 조회 
         * 4) 각각 채팅방의 count조회. 
         */
        public void HandleFEUserStatus(int bodyLength)
        {
            logger.Debug("[ce_handler][HandleFEUserStatus()] FE별 사용자수 조회 시작");

            CBFEUserStatus[] feStatusList = null;

            string[] feList = (string[]) redis.GetFEIpPortList();
            feStatusList = new CBFEUserStatus[feList.Length];

            int feUserCountSum = 0;

            for(int idx=0; idx < feList.Length; idx++)
            {
                // 2) ip:port -> fe name 조회
                string feName = redis.GetFEName(feList[idx]);

                // 3) 해당 fe에 열려있는 채팅방 번호 조회 
                int[] roomList = (int[]) redis.GetFEChattingRoomList(feName);

                // 4) 각각의 채팅방 count조회 

                foreach (int roomNo in roomList)
                {
                    feUserCountSum += redis.GetChatRoomCount(feName, roomNo);
                }
                feStatusList[idx] = new CBFEUserStatus();
                char[] feNameArr = new char[12];

                Array.Copy(feName.ToCharArray(), feNameArr, feName.ToCharArray().Length);

                feStatusList[idx].FeName = feNameArr;
                feStatusList[idx].Num = feUserCountSum;
            }// end loop

            CBHeader header = new CBHeader();
            header.Type = CBMessageType.FE_User_Status;
            header.State = CBMessageState.SUCCESS;
            header.Length = Marshal.SizeOf(feStatusList);

            Parser.Send(peer, header); 
            Parser.Send(peer, feStatusList);
            logger.Debug("[ce_handler][HandleFEUserStatus()] FE별 사용자수 조회 종료");
            return;
        }// end method


        /*
         * 1) 그냥 랭킹 조회
         */
        public void HandleChatRanking(int bodyLength)
        {
            logger.Debug("[ce_handler][HandleChatRanking()] 랭킹 조회 시작");
            int maxRange = 10;

            string[] results = (string[]) redis.GetChattingRanking(maxRange);

            CBRanking[] ranking = new CBRanking[maxRange];

            for (int idx = 1; idx <= results.Length; idx++)
            {
                ranking[idx] = new CBRanking();
                ranking[idx].Id = new char[12];

                char[] feNameArr = new char[12];
                //Array.Copy(feName.ToCharArray(), feNameArr, feName.ToCharArray().Length);
                Array.Copy(results[idx].ToCharArray(), ranking[idx].Id, results[idx].ToCharArray().Length); 
                ranking[idx].Rank = idx;
            }

            // generate Header
            CBHeader header = new CBHeader();
            header.Type = CBMessageType.Chat_Ranking;
            header.State = CBMessageState.SUCCESS;
            header.Length = Marshal.SizeOf(ranking);
            // send header
            Parser.Send(peer, header);

            // send body
            Parser.Send(peer, ranking);
            logger.Debug("[ce_handler][HandleChatRanking()] 랭킹 조회 종료");
            return;
        }// end method 


    }// end class



}
