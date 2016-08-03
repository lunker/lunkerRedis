using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public struct CFJoinFailBody
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
    public char[] ip;
    public int port;


    public CFJoinFailBody(char[] ip, int port)
    {
        this.ip = new char[15];
        this.port = port;
    }

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