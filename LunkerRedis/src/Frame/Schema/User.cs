using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunkerRedis.src.Frame
{
    public struct User
    {
        private string _id;
        private string _password;

        public string Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        public string Password
        {
            get { return this._password; }
            set { this._password = value; }
        }

        public User(string id, string password)
        {
            _id = id;
            _password = password;
        }
    }// end class
}
