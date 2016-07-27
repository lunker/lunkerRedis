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
           Parser.ByteToStructure(null, typeof(Header));

           Console.Write("메세지 입력 : " );
           string content = Console.ReadLine();
           byte[] contentArr = Encoding.UTF8.GetBytes(content);
           Header header = new Header((short) MessageType.Types.CHAT_MSG, contentArr.Length);

           Console.WriteLine("크기: " + Marshal.SizeOf(header));

           Socket peer = new Socket(SocketType.Stream, ProtocolType.Tcp);
           peer.Connect(IPAddress.Parse("127.0.0.1"),MyConst.CLIENT_PORT);

           if(peer.Connected)
               Console.WriteLine("연결ㅇ");
           else
               Console.WriteLine("여ㅑㄴ결x");

           //Header header = new Header(MessageType._CHAT_MSG, contentArr.Length);

           peer.Send(Parser.StructureToByte(header));
           peer.Send(contentArr);

           Console.WriteLine("전송완료");
           

        }
    }
}
