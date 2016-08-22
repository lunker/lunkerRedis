using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using LunkerRedis.src.Utils;
using LunkerRedis.src.Common;

using System.Xml;
using log4net;
using LunkerLibrary.common.protocol;
using LunkerRedis.src.Frame.FE_BE;

namespace LunkerRedis.src
{
    public class RedisClient
    {

        private ConnectionMultiplexer _redis = null;
        private IDatabase db = null;

        private ILog logger = FileLogger.GetLoggerInstance();

        public RedisClient() { }

        public ConnectionMultiplexer Redis
        {
            get { return this._redis; }
        }
        
         /// <summary>
         /// Connect to redis server
         /// </summary>
         /// <returns>return result. true: Connected. false : fail</returns>
        public bool Connect()
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect(MyConst.redisConfig);
                db = _redis.GetDatabase();
                logger.Debug("redis connect success!!!");
                Console.WriteLine("redis: connect");
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }// end method

        public bool CheckConnection()
        {
            if (_redis.IsConnected)
                return true;
            else
                return Connect();
        }

        /// <summary>
        /// Release redis connection
        /// </summary>
        public void Release()
        {
            if (_redis != null)
            {
                _redis.Close();
                //_redis.Dispose();
                db = null;
                _redis = null;
            }
        }


        /// <summary>
        /// Add FE Server Connected Information with FE Name
        /// </summary>
        /// <param name="ip">FE Server IP</param>
        /// <param name="port">FE Server connected port</param>
        /// <returns>generated FE Server name</returns>
        public string AddFEConnectedInfo(string ip, int port)
        {
            string key = ip + Common.RedisKey.DELIMITER + port;
            string feName = "fe" + FENameGenerator.GenerateName();
            if (CheckConnection() && db.StringSet(key, feName))
                return feName;
            return "";
        }

        /// <summary>
        /// Add FE Server Connected Information with FE Name
        /// </summary>
        /// <param name="ip">FE Server IP</param>
        /// <param name="port">FE Server connected port</param>
        /// <returns>Delete result</returns>
        public bool DelFEConnectedInfo(string ip, int port)
        {
            string key = ip + Common.RedisKey.DELIMITER + port;
            if (CheckConnection())
            {
                return db.KeyDelete(key);
            }
            return false;
        }

        /// <summary>
        /// Get FE Server Info by FE server name
        /// </summary>
        /// <param name="feName">FE server name</param>
        /// <returns>FE Information</returns>
        public FEInfo GetFEInfo(string feName)
        {
            // key : 
            if (CheckConnection())
            {
                HashEntry[] feInfo = db.HashGetAll(feName);

                //Object obj = feInfo[0];
                FEInfo fe = new FEInfo();
                for (int idx = 0; idx < feInfo.Length; idx++)
                {
                    // name
                    if (feInfo[idx].Name.Equals("name"))
                    {
                        fe.Name = feInfo[idx].Value;
                    }
                    else if (feInfo[idx].Name.Equals("ip"))
                    {
                        // ip 
                        fe.Ip = feInfo[idx].Value;
                    }
                    else
                    {
                        //pot
                        fe.Port = Int32.Parse(feInfo[idx].Value);
                    }
                }
                return fe;

            }// end if
            else
                return null;
        }
      
        /// <summary>
        /// Delete FE Server Information
        /// </summary>
        /// <param name="ip">FE server ip</param>
        /// <param name="port">FE server service port</param>
        /// <returns>delete result </returns>
        public bool DelFEInfo(string ip, int port)
        {
            string key = ip + Common.RedisKey.DELIMITER + port;
            if (CheckConnection())
            {
                return db.KeyDelete(key);
            }
            return false;
        }

        /// <summary>
        /// Add FE Server Service Information
        /// </summary>
        /// <param name="feName">FE Server name</param>
        /// <param name="ip">FE Server ip</param>
        /// <param name="port">FE Server service port</param>
        public void AddFEServiceInfo(string feName, string ip, int port)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.FEServiceInfo;
            if (CheckConnection())
            {
                HashEntry[] entries = new HashEntry[2];
                entries[0] = new HashEntry("ip", ip);
                entries[1] = new HashEntry("port", port);

                db.HashSet(key, entries);
            }
        }

        /// <summary>
        /// Get FE Server Service Information
        /// </summary>
        /// <param name="feName">FE Server name</param>
        /// <returns>FE Server Service Information Array</returns>
        public object GetFEServiceInfo(string feName)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.FEServiceInfo;

            if (CheckConnection())
            {
                HashEntry[] feServiceInfo = db.HashGetAll(key);
                FEInfo feService = new FEInfo();

                foreach (HashEntry info in feServiceInfo)
                {
                    if (info.Name.Equals("ip"))
                        feService.Ip = info.Value;
                    else
                        feService.Port = (int)info.Value;
                }

                return feService;
            }
            else
                return null;
          
        }

        /// <summary>
        /// Del FE Server Service Information
        /// </summary>
        /// <param name="feName">FE Server name</param>
        /// <returns>Delete result</returns>
        public bool DelFEServiceInfo(string feName)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.FEServiceInfo;
            if (CheckConnection())
            {
                return db.KeyDelete(key);
            }
            return false;
        }

        /*
         * FE에 해당 채팅방이 있는지 확인 
         * return bool 
         * true: 존재 
         * false: 존재하지 않음 
         */
         /// <summary>
         /// Check FE has room 
         /// </summary>
         /// <param name="feName">FE Server name</param>
         /// <param name="roomNo">chatting room no</param>
         /// <returns>true: has, false: none</returns>
        public bool HasChatRoom(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.ChattingRoomList;
            if (CheckConnection())
            {
                return db.SetContains(key, roomNo);
            }
            return false;
        }

        /// <summary>
        /// Add Available FE server info(ip:port) to list
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns>add result </returns>
        public bool AddFEList(string ip, int port) {
            string key = "fe:list";
            string value = ip + Common.RedisKey.DELIMITER + port;
            if (CheckConnection())
            {
                return db.SetAdd(key, value);
            }
            return false;
        }

        /*
         * return fe ip:port가 들어있는 list 
         */
         /// <summary>
         /// Get Available FE Server list
         /// </summary>
         /// <returns>FE server info list</returns>
        public object GetFEIpPortList()
        {
            string KEY = "fe:list";
            string[] feList = null;
            if (CheckConnection())
            {
                RedisValue[] result = db.SetMembers(KEY);

                // result => ip:port 임
                feList = new string[result.Length];
                for (int idx = 0; idx < result.Length; idx++)
                {
                    feList[idx] = result[idx];
                }

                return feList;
            }
            return null;
        }
        
        /*
         * FE list에서 fe 삭제 
         * parameter : ip:port
         */
         /// <summary>
         /// Del FE from available FE List
         /// </summary>
         /// <param name="ip">FE Server Ip</param>
         /// <param name="port">FE Server port</param>
         /// <returns>delete result</returns>
        public bool DelFEList(string ip, int port)
        {
            string key = "fe:list";
            string value = ip + Common.RedisKey.DELIMITER + port;
            if (CheckConnection())
            {
                return db.SetRemove(key, value);
            }
            else
                return false;

        }


         /// <summary>
         /// After Login success, Cache user number id 
         /// </summary>
         /// <param name="key">user id</param>
         /// <param name="value">user number id</param>
         /// <returns>result </returns>
        public bool AddUserNumIdCache(string key, int value)
        {
            if (CheckConnection())
            {
                return db.StringSet(key, value);
            }
            return false;
        }

        /// <summary>
        /// Get cached user number id
        /// </summary>
        /// <param name="key">user id</param>
        /// <returns>user number id</returns>
        public int GetUserNumIdCache(string key)
        {
            if (CheckConnection())
            {
                return (int)db.StringGet(key);
            }
            return -1;   
        }

        /// <summary>
        /// Delete cached user num id
        /// </summary>
        /// <param name="key">user id</param>
        /// <returns>delete result</returns>
        public bool DelUserNumIdCache(string key)
        {
            if (CheckConnection())
            {
                return db.KeyDelete(key);
            }
            return false;
        }   

        
         /// <summary>
         /// Get FE Name by FE server ip, port
         /// </summary>
         /// <param name="feIpPort">FE server ip:port</param>
         /// <returns>fe name</returns>
        public string GetFEName(String feIpPort)
        {
            string key = feIpPort;
            if (CheckConnection())
            {
                string feName = db.StringGet(key);
                return feName;
            }
            return "";         
        }

        public int GetFETotalChatRoomCount(string feName)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.ChattingRoomList;
            if (CheckConnection())
            {
                int roomCount = (int)db.SetLength(key);
                return roomCount;
            }
            return -1;
        }

        // key : remoteName
        // value: user number id 
        public bool SetUserLogin(string remoteName, int userNumId, bool state)
        {
            string delimiter = ":";
            string login = "login";
            StringBuilder sb = new StringBuilder();

            sb.Append(remoteName);
            sb.Append(delimiter);
            sb.Append(login);

            string key = sb.ToString();
            if (CheckConnection())
            {
                bool result = db.StringSetBit(key, userNumId, state);
               
                return result;
            }
            return false;

        }

        public bool GetUserLogin(string remoteName, int userNumId)
        {
            string delimiter = ":";
            string login = "login";
            StringBuilder sb = new StringBuilder();

            sb.Append(remoteName);
            sb.Append(delimiter);
            sb.Append(login);

            string key = sb.ToString();
            if (CheckConnection())
            {

            }
            bool result = db.StringGetBit(key, userNumId);

            if (result)
            {
                logger.Debug($"[{remoteName}][Redis][GetUserLogin()] result : {result}");
            }
            else
                logger.Debug($"[{remoteName}][Redis][GetUserLogin()] result : {result}");
            return result;
        }

        public int GetUserLoginCount(string remoteName)
        {
            string key = remoteName + Common.RedisKey.DELIMITER + Common.RedisKey.Login;
            if (CheckConnection())
            {

            }
            return (int) db.StringBitCount(key);
        }
        public bool DelUserLoginKey(string remoteName)
        {
            string delimiter = ":";
            string login = "login";
            StringBuilder sb = new StringBuilder();

            sb.Append(remoteName);
            sb.Append(delimiter);
            sb.Append(login);

            string key = sb.ToString();
            if (CheckConnection())
            {
                return db.KeyDelete(key);
            }
            return false;
        }

        public bool SetUserType(string id, bool userType )
        {
            int userNumId = GetUserNumIdCache(id);
            if (CheckConnection())
            {
                return db.StringSetBit(Common.RedisKey.Dummy, userNumId, userType);
            }
            return false;
        }

        /*
         * Return user is dummy 
         * 
         * true: dummy
         * false: normal user
         */
        public bool GetUserType(string id)
        {
            int userNumId = GetUserNumIdCache(id);
            if (CheckConnection())
            {
                return db.StringGetBit(Common.RedisKey.Dummy, userNumId);
            }
            return false;
            
        }

        /*
         * Create Room 
         * 1) GET USER NUM_ID FROM CACHE
         * 2) USER가 접속해 있는 FE이름 가져오기 
         * 3) 채팅방 번호 생성 
         * 4) FE# 의 채팅방 리스트에 추가 
         * 5) fe#:room#:count metadata 추가 
         * 
         * -----------
         * 
         * 1) 채팅방 번호 생성 
         * 2) feName:chattingroomlist 에 방번호 추가 
         * 3) feName:room No:count 생성 
         * 4) feName:room No:user -> 안만들어도 될듯. 
         * 
         * Return : int - Chat Room Number
         */
        public int CreateChatRoom(string feName, string id)
        {
            StringBuilder sb = new StringBuilder();

            // 3) 채팅방 번호 생성 
            int roomNo = ChatRoomNumberGenerator.GenerateRoomNo();
            
            // 4) FE의 채팅방 목록에 추가 

            sb.Append(feName);
            sb.Append(Common.RedisKey.DELIMITER);
            sb.Append(Common.RedisKey.ChattingRoomList);

            string key = sb.ToString();

            db.SetAdd(key, roomNo);

            // 5) metadata 추가 
            // fe#:room#:count 
            NewChatRoomCount(feName, roomNo);

            return roomNo;
        }// end method

        /*
         * Add user to Chat room 
         * 채팅방에 입장.
         */
        public bool AddUserChatRoom(string feName, int roomNo, string id)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.User;
            if (CheckConnection())
            {

            }
            return db.SetAdd(key,id);
        }

        public bool IsUserEnteredChatRoom(string feName, int roomNo, string id)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.User;
            if (CheckConnection())
            {

            }
            return db.SetContains(key, id);
        }

        /*
         * 채팅방의 유저 목록에서 유저 삭제 
         */
        public bool LeaveChatRoom(string feName, int roomNo, string id)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.User;

            return db.SetRemove(key, id);
        }

        public bool DelUserChatRoomKey(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.User;
            return db.KeyDelete(key);
        }



        /*
         * return :방 나간 이후의 남아있는 유저의 수 
         */
        public int DecChatRoomCount(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.Count;
            int result = (int)db.StringDecrement(key, 1);

            logger.Debug($"[MySQL][DecChatRoomCount()][{feName}][{roomNo}] 방 나가기 연산의 결과 : {result} ");
            return result;
        }

        public bool NewChatRoomCount(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.Count;
            if (CheckConnection())
            {
                return db.StringSet(key, 0);
            }
            return false;
        }
        
        public void IncChatRoomCount(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.Count;
            if (CheckConnection())
            {
                db.StringIncrement(key, 1);
            }
        }
        

        /// <summary>
        /// Get Connected User Count in ChattingRoom 
        /// </summary>
        /// <param name="feName">frontend name</param>
        /// <param name="roomNo">room number</param>
        /// <returns>total count</returns>
        public int GetChatRoomCount(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.Count;
            if (CheckConnection())
            {
                return (int)db.StringGet(key);
            }

            return -1;
        }
       
        public bool DelChattingRoomCountKey(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.Count;
            if (CheckConnection())
            {
                return db.KeyDelete(key);
            }
            return false;
        }

        public bool DelFEChattingRoomListKey(string feName)
        {
            string chattingRoomList = "chattingroomlist";
            string key = feName + Common.RedisKey.DELIMITER + chattingRoomList;

            return db.KeyDelete(key);
        }

        public object GetFEChattingRoomList(string feName)
        {
            int[] roomList = null;

            StringBuilder sb = new StringBuilder();
            string chattingRoomList = "chattingroomlist";

            sb.Append(feName);
            sb.Append(Common.RedisKey.DELIMITER);
            sb.Append(chattingRoomList);

            string key = sb.ToString();

            roomList = new int[db.SetLength(key)];

            RedisValue[] values = db.SetMembers(key);

            for (int idx = 0; idx < values.Length; idx++)
            {
                roomList[idx] = (int)values[idx];
            }

            return roomList;
        }

        public bool DelChattingRoom(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.ChattingRoomList;
            return db.SetRemove(key, roomNo);
        }

        /*
         * 더미가 아닌 사용자의 채팅 건수를 저장 
         */
        public void AddChat(string id)
        {
            db.SortedSetIncrement(Common.RedisKey.Ranking_Chatting, id, 1);
           
        }// end method

        public object GetChattingRanking(int range)
        {
            // user id의 배열 
            SortedSetEntry[] ranks = db.SortedSetRangeByRankWithScores(Common.RedisKey.Ranking_Chatting, 0, range, Order.Descending);
        
            return ranks;
        }

        /*
         * Clear redis server 
         */
        public void ClearDB()
        {
            logger.Debug("ClearDB() before exit()");
            if (Redis != null)
            {
                string ip = "";
                int port = 0;

                XmlTextReader reader = new XmlTextReader("config\\RedisConfig.xml");
                int index = 0;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            if (reader.Name.Equals("RedisConfig"))
                                continue;
                            break;
                        case XmlNodeType.Text: //Display the text in each element.
                                               //sb.Append(reader.Value);
                            if (index == 0)
                                ip = reader.Value;
                            else
                                port = int.Parse(reader.Value);
                            index++;
                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                            if (reader.Name.Equals("RedisConfig"))
                                continue;
                            break;
                    }
                }// end while 

                Redis.GetServer(ip, port).FlushDatabase();
            }
        }


        public int GetUserEnteredRoomNo(string feName, string id)
        {
            int[] roomNoList =(int[]) GetFEChattingRoomList(feName);

            foreach(int roomNo in roomNoList)
            {
                //string key = feName + Common.RedisKey.DELIMITER + ;
                if (IsUserEnteredChatRoom(feName, roomNo, id))
                    return roomNo;
            }
            return -1;
        }

    }
}
