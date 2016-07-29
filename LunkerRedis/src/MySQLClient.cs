using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;


namespace LunkerRedis.src
{
    class MySQLClient
    {
        private MySqlConnection Conn = null;

        public void Connect()
        {
            string config = "";
            config = "server=192.168.56.190;uid=lunker;pwd=dongqlee;database=test";

            try
            {
                Conn = new MySqlConnection();
                Conn.ConnectionString = config;
                Conn.Open();
                Console.WriteLine("[MYSQL] open connection");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine("[MYSQL] open connection fail");
            }
        }


    }
}
