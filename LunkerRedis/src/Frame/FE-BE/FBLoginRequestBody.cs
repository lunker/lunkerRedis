using System.Runtime.InteropServices;

struct FBLoginRequestBody{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] char[] id;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] char[] password;
    bool isDummy;

    public char[] Id
    {
        get { return this.id; }
    }

    public char[] Password
    {
        get { return this.password; }
    }

    public bool IsDummy
    {
        get { return this.isDummy; }
    }
}
