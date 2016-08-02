﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using LunkerRedis.src.Utils;
using LunkerRedis.src.Common;
using LunkerRedis.src.Frame.FE_BE;

using log4net;

namespace LunkerRedis.src
{
    /*****
     * 
     * 함수 명 다시 ...
     */
    class RedisClient
    {
        const string REDIS_IP = "192.168.56.102";
        const int REDIS_PORT = 6379;
        private ConnectionMultiplexer _redis = null;
        private IDatabase db = null;
        private ISubscriber pubsub = null;

        //private ILog logger = FileLogger.GetLoggerInstance();


        public void Start() { }

        /*
        public static ConnectionMultiplexer GetRedisClient()
        {
            if (_redis != null)
                return _redis;

            _redis = ConnectionMultiplexer.Connect("192.168.56.102:6379" + ",allowAdmin=true,password=ldk201120841");
            return _redis;
        }
        */

        public ConnectionMultiplexer Redis
        {
            get { return this._redis; }
        }

        /*
         * Connect to redis server
         * 
         *  cache fe list from DB
         */
        public bool Connect()
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect("192.168.56.102:6379"+ ",allowAdmin=true,password=ldk201120841");
                pubsub = _redis.GetSubscriber();
                db = _redis.GetDatabase();
                Console.WriteLine("[RedisClient] Connect Success");
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }// end method

        /*
         * Get FE Server Info
         * return FEInfo Struct 
         */
        public FEInfo GetFEInfo(string feName)
        {
            // key : 
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
                    //prot
                    fe.Port = Int32.Parse(feInfo[idx].Value);
                }

            }
            return fe;
        }

        public string AddFEInfo(string ip, int port)
        {
            string key = ip + Common.RedisKey.DELIMITER + port;
            string feName = "fe" + FENameGenerator.GenerateName();
            if (db.StringSet(key, feName))
                return feName;
            return "";
        }

        /*
         * FE에 해당 채팅방이 있는지 확인 
         * return bool 
         * true: 존재 
         * false: 존재하지 않음 
         */
        public bool HasChatRoom(string fe, int roomNo)
        {
            return db.SetContains(fe, roomNo);
        }

        public bool AddFEList(string ip, int port) {
            string key = "fe:list";
            string value = ip + Common.RedisKey.DELIMITER + port;
            return db.SetAdd(key, value);
        }

        public object GetFEList()
        {
            string KEY = "fe:list";
            string[] feList = null;

            RedisValue[] result = db.SetMembers(KEY);

            // result => ip:port 임
            feList = new string[result.Length];
            for (int idx = 0; idx < result.Length; idx++)
            {
                feList[idx] = db.StringGet((string)result[idx]);
            }

            return feList;
        }
        
        public bool DelFEList(string ip, int port)
        {
            return false;
        }

        /*
         * 
         * 로그인 후 , 사용자의 num id를 캐시해둠.
         * key :id 
         * value :number id 
         */
        public bool AddUserNumIdCache(string key, int value)
        {
            return db.StringSet(key,value);
        }

        public int GetUserNumIdCache(string key)
        {
            return (int) db.StringGet(key);
        }

        public bool DelUserNumIdCache(string key)
        {
            return db.KeyDelete(key);
        }   

        /*
         *  현재 이용 가능한 FE의 이름 목록 반환 
         *  return : FE의 이름. fe1, fe2 . . . . 
         */
        

        public string GetFEName(String ip)
        {
            string key = ip;

            string feName = db.StringGet(key);
            return feName;
        }

        public int GetFERoomNum(string feName)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.ChattingRoomList;
            int roomCount = (int)db.SetLength(key);
            return roomCount;
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
            
            return db.StringSetBit(key, userNumId, state);
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

            return db.StringGetBit(key, userNumId);
        }

        public bool SetUserType(string id, bool userType )
        {
            int userNumId = GetUserNumIdCache(id);

            return db.StringSetBit(Common.RedisKey.Dummy, userNumId, userType);
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

            return db.StringGetBit(Common.RedisKey.Dummy, userNumId);
        }

        /*
         * Create Room 
         * 1) GET USER NUM_ID FROM CACHE
         * 2) USER가 접속해 있는 FE이름 가져오기 
         * 3) 채팅방 번호 생성 
         * 4) FE# 의 채팅방 리스트에 추가 
         * 5) fe#:room#:count metadata 추가 
         * 
         * Return : int - Chat Room Number
         */
        public int CreateChatRoom(string id)
        {
            StringBuilder sb = new StringBuilder();

            // 1) get user num_id
            //db.StringGet();            
            int numId = (int) db.StringGet(id);

            // 2) user가 들어있는 FE 이름 가져오기 
            string FE1 = "fe1";
            string FE2 = "fe2";
            string FEName = "";

            if(GetUserLogin(FE1,numId))
            {
                FEName = FE1;
            }
            else
            {
                FEName = FE2;
            }

            // 3) 채팅방 번호 생성 
            int roomNo = ChatRoomNumberGenerator.GenerateRoomNo();

            Console.WriteLine("Generated room number : " + roomNo);

            //logger.Debug("generate room number : " + roomNo);
            // 4) FE의 채팅방 목록에 추가 

            string Delimiter = ":";
            string ChattingRoomList = "chatroomlist";
            string Key = "";

            sb.Append(FEName);
            sb.Append(Delimiter);
            sb.Append(ChattingRoomList);

            /*
            if (db.SetAdd(Key, roomNo))
                return roomNo;
            */
            return roomNo;

            // 5) metadata 추가 
            // fe#:room#:count 
            // fe#:room#:user  


        }// end method
        public void JoinChatRoom()
        {

        }

        /*
         * 채팅방의 유저 목록에서 유저 삭제 
         */
        public bool LeaveChatRoom(string feName, string id, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.User;

            return db.SetRemove(key, id);
        }

        /*
         * return :방 나간 이후의 남아있는 유저의 수 
         */
        public int DecChatRoomCount(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.Count;

            return (int) db.StringDecrement(key,1);
        }

        public void IncChatRoomCount(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.Count;

            //return (int)db.StringIncrement(key, 1);
        }
        
        public int GetChatRoomCount(string feName, int roomNo)
        {
            string key = feName + Common.RedisKey.DELIMITER + Common.RedisKey.Room + roomNo + Common.RedisKey.DELIMITER + Common.RedisKey.Count;

            return (int) db.StringGet(key);
        }
        /*
         * return room number list 
         * return : int[] 
         */
        public Object ListChatRoom(string fe)
        {
            int[] roomList = null;

            
            StringBuilder sb = new StringBuilder();
            string delimiter = ":";
            string chattingRoomList = "chattingroomlist";

            sb.Append(fe);
            sb.Append(delimiter);
            sb.Append(chattingRoomList);
            
            string key = sb.ToString();

            roomList = new int[db.SetLength(key)];
        
            RedisValue[] values = db.SetMembers(key);

            for(int idx=0; idx<values.Length; idx++)
            {
                roomList[idx] = (int) values[idx];
            }

            return roomList;
        }

        /*
         * 더미가 아닌 사용자의 채팅 건수를 저장 
         */
        public void AddChat(string id)
        {

            db.SortedSetIncrement(Common.RedisKey.Ranking_Chatting, id, 1);
            /*
            //bool result = false;
            db.GetS
            //result = db.SetAdd(chat, chat);
            //db.StringSet(chat, chat);
            db.SortedSetIncrement(Common.RedisKey.Ranking_Chatting, id, 1);
            */
        }// end method

        public object GetChattingRanking(int range)
        {
            
            // user id의 배열 
            RedisValue[] ranks = db.SortedSetRangeByRank(Common.RedisKey.Ranking_Chatting, 0, range, Order.Descending);
            return ranks;
        }

    }
}
