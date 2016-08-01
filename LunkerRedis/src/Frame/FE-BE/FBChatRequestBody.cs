
using System.Runtime.InteropServices;

struct FBChatRequestBody{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] char[] id;

    public char[] Id
    {
        get { return this.id; }
    }
}
