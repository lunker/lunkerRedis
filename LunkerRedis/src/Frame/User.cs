using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Frame
{
    class User
    {
        string _id;
        string _password;


        public string Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        public User(string id, string password)
        {
            _id = id;
            _password = password;
        }
       
    }// end class
}
