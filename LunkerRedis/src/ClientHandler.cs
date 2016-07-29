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
        private Socket Peer = null;

        public ClientHandler() { }
        public ClientHandler(Socket handler)
        {
            this.Peer = handler;
        }

        /**
         * Monitoring Client의 request 처리 
         */
        public void HandleRequest()
        {
            // Read Request
            Header header;
            byte[] bodyArr = null;

            header = (Header) Parser.Read(Peer, MyConst.HEADER_LENGTH, typeof(Header));

            switch (header.Type)
            {
               
            }
            //bodyArr = Parser.Read(Peer);
            // Logic 

            // Send Response
            //Parser.Send();
        }// end method 


        public void CheckUserID(string userId)
        {
            //
        }

        public void HandleSignupRequest()
        {

        }

        public void HandleLoginRequest()
        {

        }

        public void HandleListChatRoomRequest()
        {

        }

        public void HandleEnterChatRoomReqeust()
        {

        }
        
        public void HandleLeaveChatRoomRequest()
        {

        }

        public void HandleCreateChatRoomRequest()
        {

        }

        public void HandleChatting()
        {

        }
    }// end class



}
