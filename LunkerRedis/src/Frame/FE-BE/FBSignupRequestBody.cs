using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


struct FBSignupRequestBody
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    char[] id;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    char[] password;

    public char[] Id
    {
        get { return this.id; }
    }

    public char[] Password
    {
        get { return this.password; }
    }
}
