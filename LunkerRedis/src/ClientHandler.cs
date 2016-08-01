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

namespace LunkerRedis.src
{
    class ClientHandler
    {
        private Socket peer = null;
        private RedisClient redis = null;
        private MySQLClient mysql = null;

        public ClientHandler() { }
        public ClientHandler(Socket handler)
        {
            this.peer = handler;
            redis = new RedisClient();
            //mysql = new MySQLClient();
        }

        /**
         * Monitoring Client의 request 처리 
         */
        public void HandleRequest()
        {

            while (true)
            {
                // Read Request

                try
                {
                    CBHeader header;
                    header = (CBHeader)Parser.Read(peer, (int)ProtocolHeaderLength.FBHeader, typeof(CBHeader));

                    switch (header.type)
                    {
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
                    return;
                }
            }//end loop

        }//end method

        /*
         * 전체 채팅방 수 조회 
         * 1) 
         */
        public void HandleRequestTotalRoomCount(int bodyLength)
        {
            string[] feList = (string[]) redis.GetFEList();
            int sum = 0;
            foreach (string fe in feList)
            {
                string feName = redis.GetFEName(fe);

                sum += redis.GetFERoomNum(feName);
            }

            CBHeader header = new CBHeader();
            header.Type = CBMessageType.Total_Room_Count;
            header.State = CBMessageState.SUCCESS;

            byte[] data = BitConverter.GetBytes(sum);

            header.Length = data.Length;

            Parser.Send(peer, header);
            Parser.Send(peer, data);
        }

        public void HandleFEUserStatus(int bodyLength)
        {

        }

        public void HandleChatRanking(int bodyLength)
        {

        }


    }// end class



}
