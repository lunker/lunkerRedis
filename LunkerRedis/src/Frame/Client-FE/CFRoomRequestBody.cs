 using System.Runtime.InteropServices;
 struct CFRoomRequestBody
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] char[] id;
    int roomNo; 
}
