using LunkerRedis.src.Utils;
using LunkerRedis.src.Frame;
using LunkerRedis.src.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {


         
                Test();

            }
           
        }
        public static void Test()
        {
            Console.Write("메세지 입력 : ");
            string content = Console.ReadLine();



            content += '\0';
            if (content.Contains('\0'))
            {
                Console.WriteLine("has nulll");
            }
            else
                Console.WriteLine("no nulll");

            
            byte[] contentArr = Encoding.UTF8.GetBytes(content);

            Console.WriteLine("[client] 입력받은 문자열의 크기: "+ contentArr.Length * sizeof(char));

            //Console.WriteLine(sizeof(content));

            Header header = new Header((short)MessageType.Types.CHAT_MSG, contentArr.Length * sizeof(char));
            MessageFrame message = new MessageFrame(content);

            Console.WriteLine("[client] header의 크기: " + Marshal.SizeOf(header));
            //Console.WriteLine("크기: " + Marshal.SizeOf(message));

            Socket peer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            peer.Connect(IPAddress.Parse("127.0.0.1"), MyConst.CLIENT_PORT);

            if (peer.Connected)
                Console.WriteLine("연결ㅇ");
            else
                Console.WriteLine("여ㅑㄴ결x");

            peer.Send(Parser.StructureToByte(header));

            peer.Send(Parser.StructureToByte(message));
            //peer.Send(contentArr);

            Console.WriteLine("전송완료");
        }
    }
}
