using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct FBHeader
{
    public FBMessageType type;
    public FBMessageState state;
    public int length;
    public int sessionId;

    public int Length
    {
        get { return this.length; }
        set { this.length = value; }
    }

    public FBMessageType Type
    {
        get { return this.type; }
        set { this.type = value; }
    }

    public FBMessageState State
    {
        get { return this.state; }
        set { this.state = value; }
    }

    public int SessionId
    {
        get { return this.sessionId; }
        set { this.sessionId = value; }
    }
}

public enum FBMessageType : short
{
    Id_Dup = 110,
    Signup = 120,

    Login = 210,
    Logout = 220,

    Room_Create = 310,
    Room_Leave = 320,
    Room_Join = 330,
    Room_List = 340,

    Chat_Count = 410,

    Health_Check = 510,

    Connection_Info = 610
};

public enum FBMessageState : short
{
    REQUEST = 100,
    SUCCESS = 200,
    FAIL = 400
}
