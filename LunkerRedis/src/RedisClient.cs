using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using LunkerRedis.src.Utils;
using LunkerRedis.src.Common;

namespace LunkerRedis.src
{
    class RedisClient
    {
        const string REDIS_IP = "192.168.56.102";
        const int REDIS_PORT = 6379;
        private ConnectionMultiplexer _redis = null;
        private IDatabase db = null;
        private ISubscriber pubsub = null;

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
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }// end method

        public string GetFEName(string ip)
        {
            RedisValue result =  db.StringGet(ip);

            if (result == RedisValue.Null)
                return "";
            else
                return (string)result;
        }// end method


        public bool CheckIdDup(string userId)
        {
            //db.SetContains("user", );

            return false;
        }

        public bool AddUserCache(string key, int value)
        {
            return db.StringSet(key,value);
            //redis.AddUserCache(result.Id, result.NumId);
        }

        /*
         *  return FE Name List 
         */
        public object GetFENameList()
        {
            string KEY = "fe:list";
            string[] feList = null;

            RedisValue[] result = db.HashKeys(KEY);

            feList = new string[result.Length];
            for (int idx=0; idx<result.Length; idx++)
            {
                feList[idx] = result[idx];
            }

            return feList;
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

            if(db.StringGetBit(FE1, numId))
            {
                FEName = FE1;
            }
            else
            {
                FEName = FE2;
            }

            // 3) 채팅방 번호 생성 
            int roomNo = NumberGenerator.GenerateRoomNo();

            // 4) FE의 채팅방 목록에 추가 

            string Delimiter = ":";
            string ChattingRoomList = "chatroomlist";
            string Key = "";

            sb.Append(FEName);
            sb.Append(Delimiter);
            sb.Append(ChattingRoomList);
        
            
            if (db.SetAdd(Key, roomNo))
                return roomNo;
            else
                return -1; // 방 번호 중복 시 예외 처리 

            // 5) metadata 추가 
            // fe#:room#:count 
            // fe#:room#:user  
            



        }// end method


        public void JoinChatRoom()
        {

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

        public void AddChat(string id)
        {
            //bool result = false;
            db.GetS
            //result = db.SetAdd(chat, chat);
            //db.StringSet(chat, chat);
            db.SortedSetIncrement(Common.RedisKey.Ranking_Chatting, id, 1);
        }// end method

     

    }
}
