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
using System.Timers;
using System.Threading;

using StackExchange.Redis;
using LunkerLibrary.common.Utils;

namespace LunkerRedis.src
{
    class ClientHandler
    {
        private ILog logger = FileLogger.GetLoggerInstance();

        private Socket peer = null;
        private RedisClient redis = null;
        private MySQLClient mysql = null;

        private bool threadState = MyConst.Run;
        private int healthCount = 0;

        public ClientHandler() { }
        public ClientHandler(Socket handler)
        {
            this.peer = handler;
            redis = RedisClientPool.GetInstance();
            mysql = MySQLClientPool.GetInstance();
        }

        /*
        /// <summary>
        /// Send Health Check Message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SendHealthCheck(object sender, ElapsedEventArgs e)
        {
            Interlocked.Increment(ref healthCount);

            if (healthCount == 3)
            {
                // time-out
                peer.Close();
                threadState = MyConst.Exit;
                return;
            }

            CBHeader header = new CBHeader();
            header.Type = MessageType.Health_Check;
            header.State = MessageState.REQUEST;
            header.Length = 0;

            NetworkManager.Send(peer, header);

            return;
        }
        */


        /// <summary>
        /// Handle Health Check Receive
        /// </summary>
        public void HandleHealthCheck()
        {
            healthCount = 0;
        }

        /// <summary>
        /// Stop Thread
        /// </summary>
        public void HandleStopThread()
        {
            threadState = MyConst.Exit;
        }
        /*
        /// <summary>
        /// Monitoring Client의 request 처리 
        /// </summary>
        public void HandleRequest()
        {
            logger.Debug("[ce_handler][HandleRequest()]");
           

            while (threadState)
            {
                // Read Request
                try
                {
                    CBHeader header;
                    header = (CBHeader)NetworkManager.Read(peer, (int)ProtocolHeaderLength.CBHeader, typeof(CBHeader));

                    switch (header.type)
                    {
                        
                        case CBMessageType.Health_Check:
                            HandleHealthCheck();
                            break;
                        
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
                            HandleError();
                            break;
                    }
                }
                catch (SocketException se)
                {
                    peer.Close();

                    logger.Debug("[ce_handler][HandleRequest()] Client Handler Exit.");
                    break;
                }
                finally
                {
                    RedisClientPool.ReleaseObject(redis);
                    MySQLClientPool.ReleaseObject(mysql);
                }
            }//end loop

            RedisClientPool.ReleaseObject(redis);
            MySQLClientPool.ReleaseObject(mysql);
            logger.Debug("[ce_handler][HandleRequest()] stop thread.");
            return;
        }//end method

        public void HandleLogin(int bodyLength)
        {
            logger.Debug("[ce_handler][HandleLogin()] 로그인 시작");
            // read body
            CBLoginRequestBody body = (CBLoginRequestBody)NetworkManager.Read(peer, bodyLength, typeof(CBLoginRequestBody));

            string id = new string(body.Id).Split('\0')[0];// null character 
            string password = new string(body.Password).Split('\0')[0];// null character 
            CBHeader responseHeader = new CBHeader();

            if (id.Equals(""))
            {
                // 유효하지 않은 아이디 입력 
                responseHeader.Type = CBMessageType.Login;
                responseHeader.Length = 0;
                responseHeader.State = MessageState.FAIL;

                NetworkManager.Send(peer, responseHeader);
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
                responseHeader.State = MessageState.SUCCESS;
                logger.Debug("[ce_handler][HandleLogin()] 로그인 성공");
            }
            else
            {
                responseHeader.State = MessageState.FAIL;
                logger.Debug("[ce_handler][HandleLogin()] 로그인 실패");
            }

            NetworkManager.Send(peer, responseHeader);
            logger.Debug("[ce_handler][HandleLogin()] 로그인 종료");
            return;

        }
      
        /// <summary>
        /// Handle Request Total Room Count
        /// </summary>
        /// <param name="bodyLength">body message length</param>
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

            logger.Debug("[ce_handler][HandleRequestTotalRoomCount()] 전체 채팅방 수 :  " + sum);

            CBHeader header = new CBHeader();
            header.Type = CBMessageType.Total_Room_Count;
            header.State = MessageState.SUCCESS;

            byte[] data = BitConverter.GetBytes(sum);

            header.Length = data.Length;

            NetworkManager.Send(peer, header);
            NetworkManager.Send(peer, data);
            logger.Debug("[ce_handler][HandleRequestTotalRoomCount()] 전체 채팅방 개수 조회 종료");
            return;
        }// end method


        /// <summary>
        /// HandleRequest List FE' user count
        /// <para>1) 전체 FE IP:PORT string 가져옴 </para>
        /// <para>2) 가져온 string -> fe Name 조회</para>
        /// <para>3) fename:chattingroomlist를 통해서 채팅방 번호들 조회 </para>
        /// <para>4) 각각 채팅방의 count조회. </para>
        /// </summary>
        /// <param name="bodyLength"></param>
        public void HandleFEUserStatus(int bodyLength)
        {
            logger.Debug("[ce_handler][HandleFEUserStatus()] FE별 사용자수 조회 시작");

            CBFEUserStatus[] feStatusList = null;

            string[] feList = (string[]) redis.GetFEIpPortList();
            feStatusList = new CBFEUserStatus[feList.Length];

            for(int idx=0; idx < feList.Length; idx++)
            {
                // 2) ip:port -> fe name 조회
                string feName = redis.GetFEName(feList[idx]);

                int feConnectedUserCount = redis.GetUserLoginCount(feName);
                

                feStatusList[idx] = new CBFEUserStatus();

                FEInfo info = (FEInfo) redis.GetFEServiceInfo(feName);

                char[] ip = new char[15]; // space 할당
                char[] tmpIp = info.Ip.ToCharArray() ; // ip parsing
                int port = info.Port; // port parsing

                Array.Copy(tmpIp, ip, tmpIp.Length); // copy to space 

                feStatusList[idx].Ip = new char[15];
                Array.Copy(info.Ip.ToCharArray(), feStatusList[idx].Ip,info.Ip.ToCharArray().Length);

                feStatusList[idx].Port = port;
                feStatusList[idx].Num = feConnectedUserCount;
            }// end loop

            CBHeader header = new CBHeader();
            header.Type = CBMessageType.FE_User_Status;
            header.State = MessageState.SUCCESS;

            logger.Debug("[ce_handler][HandleFEUserStatus()] fe 갯수 : " + feStatusList.Length);
            
            byte[] feStatusListByte = NetworkManager.CBFEUserStatusStructureArrayToByte(feStatusList, typeof(CBFEUserStatus));
            header.Length = feStatusListByte.Length;


            if(header.Length == 0)
            {
                NetworkManager.Send(peer, header);
            }
            else
            {
                NetworkManager.Send(peer, header);
                NetworkManager.Send(peer, feStatusListByte);
            }

            logger.Debug("[ce_handler][HandleFEUserStatus()] FE별 사용자수 조회 종료");
            return;
        }// end method
            
        /// <summary>
        /// Handle Chat ranking request
        /// </summary>
        /// <param name="bodyLength">body message length</param>
        public void HandleChatRanking(int bodyLength)
        {
            logger.Debug("[ce_handler][HandleChatRanking()] 랭킹 조회 시작");
            int maxRange = 10;

            SortedSetEntry[] results = (SortedSetEntry[]) redis.GetChattingRanking(maxRange);

            CBRanking[] ranking = new CBRanking[results.Length];

            for (int idx = 0; idx < results.Length; idx++)
            {
                ranking[idx] = new CBRanking();
                ranking[idx].Id = new char[12];

                Array.Copy( ((string)results[idx].Element).ToCharArray(), ranking[idx].Id, ((string)results[idx].Element).ToCharArray().Length); 
                ranking[idx].Rank = idx+1;
                ranking[idx].Score = (int) results[idx].Score;
            }

            logger.Debug("[ce_handler][HandleChatRanking()] 랭킹 조회 결과 : " + results.Length);

            if (ranking.Length != 0)
            {
                byte[] rankingArr = NetworkManager.CBRankingStructureArrayToByte(ranking, typeof(CBRanking));
                // generate Header
                CBHeader header = new CBHeader();
                header.Type = CBMessageType.Chat_Ranking;
                header.State = MessageState.SUCCESS;
                header.Length = rankingArr.Length;
                NetworkManager.Send(peer, header);
                NetworkManager.Send(peer, rankingArr);
            }
            else
            {
                // send header
                CBHeader header = new CBHeader();
                header.Type = CBMessageType.Chat_Ranking;
                header.State = MessageState.SUCCESS;
                header.Length = ranking.Length;
                NetworkManager.Send(peer, header);
            }

            logger.Debug("[ce_handler][HandleChatRanking()] 랭킹 조회 종료");
            return;
        }// end method 

        public void HandleError()
        {
            logger.Debug($"[fe_handler][HandleError] 정의되지 않은 메세지  ");
            return;
        }
        */

    }// end class



}
