using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public struct FBConnectionInfo
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
    public char[] ip;
    public int port; //lenght of next body 

    public char[] Ip
    {
        get { return this.ip; }
        set { this.ip = value; }
    }

    public int Port
    {
        get { return this.port; }
        set { this.port = value; }
    }
}
