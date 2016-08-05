using System.Runtime.InteropServices;

struct FBLoginRequestBody{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] char[] id;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] char[] password;
    

    public char[] Id
    {
        get { return this.id; }
    }

    public char[] Password
    {
        get { return this.password; }
    }

    
}
