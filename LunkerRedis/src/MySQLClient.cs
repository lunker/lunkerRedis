using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using LunkerRedis.src.Frame;
using System.Data;

namespace LunkerRedis.src
{
    class MySQLClient
    {
        private MySqlConnection conn = null;

        public void Connect()
        {
            string config = "";
            config = "server=192.168.56.190;uid=lunker;pwd=dongqlee;database=test";

            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = config;
                conn.Open();
                Console.WriteLine("[MYSQL] open connection");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine("[MYSQL] open connection fail");
            }
        }

        /*
         * Do Signup 
         */
        public bool CreateUser(string id, string password)
        {
            int result = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO USER (ID, PASSWORD, REG_DATE) VALUES ");

            sb.Append("(" + id + ")");
            sb.Append("(" + password + ")");
            sb.Append("(" + "now()" + ")");


            MySqlCommand cmd = new MySqlCommand(sb.ToString(), conn);
            result = cmd.ExecuteNonQuery();
            if (result > 0)
                return true;
            else
                return false;
        }// end method 

        /*
         * GET USRE UNIQUE NUMBER ID 
         */
        public int SelectUserNumId(string id)
        {
            int numId = 0;

            // GENERATE QUERY
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT (NUM_ID) FROM USER");
            sb.Append("WHERE ID=");
            sb.Append(id);


            MySqlCommand cmd = new MySqlCommand(sb.ToString(), conn);
            //int result = cmd.ExecuteNonQuery();
            numId = (int) cmd.ExecuteScalar();

            /*
             * 예외처리 추가 
             */
            return numId;
        }

        /*
         * SELECT USER INFO BY ID 
         */
        public User SelectUserInfo(string id)
        {
            User user = new User();
            // GENERATE QUERY
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM USER");
            sb.Append("WHERE ID=");
            sb.Append(id);
            string query = sb.ToString();

            DataSet ds = new DataSet();
            MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
            da.Fill(ds);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                user.Id = (string) row["id"];
                user.Password = (string) row["password"];
            }
            
            return user;
        }
    }
}
