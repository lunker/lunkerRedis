using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public struct CBRanking
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    char[] id;

    int rank;
}