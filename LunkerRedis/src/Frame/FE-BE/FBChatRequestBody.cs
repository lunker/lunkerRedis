
using System.Runtime.InteropServices;

struct FBChatRequestBody{

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] char[] id;

    public char[] Id
    {
        get { return this.id; }
    }
}
