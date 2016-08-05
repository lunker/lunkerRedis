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

    int score;

    public char[] Id
    {
        get { return this.id; }
        set { this.id = value; }
    }

    public int Rank
    {
        get { return this.rank; }
        set { this.rank = value; }
    }


    public int Score
    {
        get { return this.score;  }
        set { this.score = value; }
    }

}