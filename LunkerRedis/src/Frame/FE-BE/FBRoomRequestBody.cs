using System.Runtime.InteropServices;

struct FBRoomRequestBody{

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    char[] id;
    int roomNo;

}
