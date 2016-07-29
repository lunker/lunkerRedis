using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
struct FBHeader
{
    public FBMessageType type;
    public FBMessageState state;
    public int length;
    public int sessionId;
}

enum FBMessageType : short
{
    Id_Dup = 110,
    Signup = 120,

    Login = 210,

    Room_Create = 310,
    Room_Leave = 320,
    Room_Join = 330,
    Room_List = 340,

    Chat_Count = 410
};

enum FBMessageState : short
{
    REQUEST = 100,
    SUCCESS = 200,
    FAIL = 400
}
