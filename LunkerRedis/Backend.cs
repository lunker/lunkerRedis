using LunkerRedis.src;
using LunkerRedis.src.Frame;
using LunkerRedis.src.Utils;

using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunkerRedis.src.Common;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace LunkerRedis
{
    class Backend
    {
        private SockListener frontendListener = null;
        private SockListener clientListener = null;

        //wha
        public void Start()
        {
            Initialize();
            
            /*
            Parser.ByteToStructure(null, typeof(Header));

            Console.Write("메세지 입력 : " );
            string content = Console.ReadLine();
            byte[] contentArr = Encoding.UTF8.GetBytes(content);
            Header header = new Header(MessageType._CHAT_MSG, contentArr.Length);

            Console.WriteLine("크기:"+Marshal.SizeOf(header));

            Socket peer = new Socket(SocketType.Stream, ProtocolType.Tcp);
            peer.Connect(IPAddress.Parse("10.100.58.9"),11000);
        
            if(peer.Connected)
                Console.WriteLine("연결ㅇ");
            else
                Console.WriteLine("여ㅑㄴ결x");

            //Header header = new Header(MessageType._CHAT_MSG, contentArr.Length);
            
            peer.Send(Parser.StructureToByte(header));
            peer.Send(contentArr);

            Console.WriteLine("전송완료");
            */

            Console.ReadLine();// wait
            Console.ReadLine();// wait
        }

        public void Initialize()
        {
            RedisClient redisClient = new RedisClient();
            if (redisClient.Connect())
            {
                Console.WriteLine("[Connect] success");
            }
            else
            {
                Console.WriteLine("[Connect] fail");
            }

            // connection for FE 
            frontendListener = new SockListener(MyConst.IP,MyConst.FRONTEND_PORT);
            frontendListener.Connect();
            Thread fListenerThread = new Thread(new ThreadStart(frontendListener.Listen));
            fListenerThread.Start();
            Console.WriteLine("[FE_HANDLER] 초기화 완료");

            // connection for Monitoring Server
            clientListener = new SockListener(MyConst.IP, MyConst.CLIENT_PORT);
            clientListener.Connect();
            Thread cListenerThread = new Thread(new ThreadStart(clientListener.Listen));
            cListenerThread.Start();
            Console.WriteLine("[CLIENT_HANDLER] 초기화 완료");
        }
    }
}
