using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public struct CBFEUserStatus
{
    /*
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    char[] feName;
    */

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
    char[] ip;
    int port;

    int num;


    /*
    public char[] FeName
    {
        get { return this.feName; }
        set { this.feName = value; }
    }
    */

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

    public int Num
    {
        get { return this.num; }
        set { this.num = value; }
    }
}