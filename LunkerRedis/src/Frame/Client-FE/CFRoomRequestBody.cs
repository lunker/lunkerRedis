 using System.Runtime.InteropServices;
 struct CFRoomRequestBody
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] char[] id;
    int roomNo; 
}
