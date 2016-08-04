using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct CBHeader
{
    public CBMessageType type;
    public CBMessageState state;
    public int length;

    public int Length
    {
        get { return this.length; }
        set { this.length = value; }
    }

    public CBMessageType Type
    {
        get { return this.type; }
        set { this.type = value; }
    }

    public CBMessageState State
    {
        get { return this.state; }
        set { this.state = value; }
    }
}

public enum CBMessageType : short
{

    Total_Room_Count = 110,
    FE_User_Status = 210,
    Chat_Ranking = 310,
    Login = 410,
    Health_Check = 510

};

public enum CBMessageState : short
{
    REQUEST = 100,
    SUCCESS = 200,
    FAIL = 400
}
