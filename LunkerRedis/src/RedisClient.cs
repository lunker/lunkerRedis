using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using LunkerRedis.src.Utils;

namespace LunkerRedis.src
{
    class RedisClient
    {
        const string REDIS_IP = "192.168.56.102";
        const int REDIS_PORT = 6379;
        private ConnectionMultiplexer _redis = null;
        private IDatabase db = null;

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
         */
        public bool Connect()
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect("192.168.56.102:6379"+ ",allowAdmin=true,password=ldk201120841");
                db = _redis.GetDatabase();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }// end method

        public bool CheckIdDup(string userId)
        {
            //db.SetContains("user", );

            return false;
        }

        /*
         * Create Room 
         * Return : int - Chat Room Number
         */
        public int CreateChatRoom()
        {
            StringBuilder sb = new StringBuilder();

            // user가 들어있는 FE 이름 가져오기 
            string FE1 = "FE1";
            string FE2 = "FE2";
            string FEName = "";
            if(db.StringGetBit(FE1, 0))
            {
                FEName = FE1;
            }
            else
            {
                FEName = FE2;
            }

            ///  FE의 채팅방 목록에 추가 
            string ChattingRoomList = "ChatRoomList";
            
            string Delimiter = ":";
            string Key = "";

            sb.Append(FEName);
            sb.Append(Delimiter);
            sb.Append(ChattingRoomList);
            int roomNo = NumberGenerator.GenerateRoomNo();

            if (db.SetAdd(Key, roomNo))
                return roomNo;
            else
                return -1; // 방 번호 중복 시 예외 처리 
        }// end method

        public void JoinChatRoom()
        {

        }

        public void ListChatRoom()
        {
            

            db.set
        }

        public bool AddChat(string chat)
        {
            bool result = false;

            result = db.SetAdd(chat, chat);
            db.StringSet(chat, chat);

            return result;
        }// end method
    }
}
