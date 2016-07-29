using LunkerRedis.src.Common;
using LunkerRedis.src.Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Utils
{
    static class Parser
    {
       

        public static object ByteToStructure(byte[] data, Type type)
        {

            IntPtr buff = Marshal.AllocHGlobal(data.Length); // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당한다.

            Marshal.Copy(data, 0, buff, data.Length); // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.
            object obj = Marshal.PtrToStructure(buff, type); // 복사된 데이터를 구조체 객체로 변환한다.

            Marshal.FreeHGlobal(buff); // 비관리 메모리 영역에 할당했던 메모리를 해제함



            if (Marshal.SizeOf(obj) != data.Length)// (((PACKET_DATA)obj).TotalBytes != data.Length) // 구조체와 원래의 데이터의 크기 비교

            {
                
                return null; // 크기가 다르면 null 리턴

            }

            return obj; // 구조체 리턴
        }// end method


        // 구조체를 byte 배열로
        public static byte[] StructureToByte(object obj)
        {
            int datasize = Marshal.SizeOf(obj);//((PACKET_DATA)obj).TotalBytes; // 구조체에 할당된 메모리의 크기를 구한다.

            IntPtr buff = Marshal.AllocHGlobal(datasize); // 비관리 메모리 영역에 구조체 크기만큼의 메모리를 할당한다.

            Marshal.StructureToPtr(obj, buff, false); // 할당된 구조체 객체의 주소를 구한다.

            byte[] data = new byte[datasize]; // 구조체가 복사될 배열

            Marshal.Copy(buff, data, 0, datasize); // 구조체 객체를 배열에 복사

            Marshal.FreeHGlobal(buff); // 비관리 메모리 영역에 할당했던 메모리를 해제함

            return data; // 배열을 리턴
        }

        /*
         * Read Message From peer 
         */
        public static Object Read(Socket peer, int length, short type)
        {
            
            Object obj = null;
            int rc = default(int);
            byte[] buff = new byte[length];
            Type objType = null;


            // get message type 
            // 뭐냐 이건...
            /*
            switch (type)
            {
                case (int)Types.Header:
                    objType = typeof(Header);
                    break;
                case (int)Types.Message:
                    objType = typeof(MessageFrame);
                    break;
            }
            */


            try
            {

                rc = peer.Receive(buff);

                if (rc == 0)
                {

                }
                else if (rc > 0)
                {

                }
                else
                {

                }

                obj = Parser.ByteToStructure(buff, objType);
                return obj;
            }
            catch (SocketException se)
            {
                return null;
            }
        }// end method

        /*
         * Send Message To Peer
         */
        public static bool Send(Socket peer, Object obj)
        {
            int rc = default(int);
            try
            {
                rc = peer.Send(StructureToByte(obj));

                if (rc == 0) {
                    Console.WriteLine("");
                }
                else if(rc > 0)
                {

                }
                else
                {

                }

                return true;
            }
            catch (SocketException se)
            {
                return false;
            }
        }// end method
    }
}
