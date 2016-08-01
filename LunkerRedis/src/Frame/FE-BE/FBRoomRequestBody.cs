using System.Runtime.InteropServices;

struct FBRoomRequestBody{

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    char[] id;
    int roomNo;

    public char[] Id
    {
        get { return this.id; }
    }
    public int RoomNo
    {
        get { return this.roomNo; }
    }
}
