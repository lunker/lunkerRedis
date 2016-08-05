using System.Runtime.InteropServices;

struct FBLoginResponseBody{
    // none
    // using Header.Status
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public char[] id;

    public char[] Id
    {
        get { return this.id; }
        set { this.id = value; }
    }


}
