using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
struct CFHeader
{
    public CFMessageType type;
    public CFMessageState state;
    public int length;
}

enum CFMessageType : short
{
    Id_Dup = 110,
    Signup = 120,

    Login = 210,

    Room_Create = 310,
    Room_Leave = 320,
    Room_Join = 330,
    Room_List = 340,

    Chat_MSG_From_Client = 410,
    Chat_MSG_Broadcast = 420,
};

enum CFMessageState : short
{
    REQUEST = 100,
    SUCCESS = 200,
    FAIL = 400
}

