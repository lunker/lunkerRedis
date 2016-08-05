using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using LunkerRedis.src.Frame;
using System.Data;
using log4net;
using log4net.Config;
using System.Data.SqlTypes;
using LunkerRedis.src.Common;
using System.Xml;

namespace LunkerRedis.src
{
    public class MySQLClient
    {
        private ILog logger = LogManager.GetLogger(MyConst.Logger);
        private string config = "";
        private MySqlConnection conn = null;

        public MySQLClient() { }

        public void Release()
        {
            logger.Debug("[MySQL][Release()] Release MySQL Client ");
            if (conn != null)
            {
                conn.Close();
                //conn.Dispose();
                conn = null;
            }
        }

        public bool Ping()
        {
            logger.Debug("[MySQL][Ping()] Ping . . .");
            if (conn != null)
            {
                if (conn.Ping())
                {
                    logger.Debug("[MySQL][Ping()] connection true : alive");
                    return true;
                }
                else
                {
                    logger.Debug("[MySQL][Ping()] connection false : die");
                    return false;
                }

            }
            else
                return false;
        }

        public void Connect()
        {
            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = MyConst.mysqlConfig;
                conn.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                return;
            }
        }
        
        /*
         *return true : 중복
         * false : 중복x
         */
        public bool CheckIdDup(string id)
        {
            
            if (!Ping())
                Connect();
            

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM USER ");
            sb.Append("WHERE id=");
            sb.Append("'");
            sb.Append(id);
            sb.Append("'");
            string query = sb.ToString();

            DataSet ds = new DataSet();
            MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                logger.Debug("[MYSQL][CheckIdDUp()] duplicate");
                return true;
            }
            else
            {
                logger.Debug("[MYSQL][CheckIdDup()] not dup");
                return false;
            }
        }

        /*
         * Do Signup 
         */
        public bool CreateUser(string id, string password, bool isDummy)
        {
            
            if (!Ping())
                Connect();
            

            logger.Info("[MySQL][CreateUser()] start");            
            int result = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO USER (ID, PASSWORD, DUMMY, REG_DATE ) VALUES ");

            sb.Append("('" + id + "',");
            sb.Append( "'" + password + "',");
            sb.Append(isDummy +",");
            sb.Append("now()" + ")");

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
            
            if (!Ping())
                Connect();
       
            int numId = 0;

            // GENERATE QUERY
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT (NUM_ID) FROM USER ");
            sb.Append("WHERE ID=");
            sb.Append("'" + id +"'");

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

            
            if (!Ping())
                Connect();
            
            lock (this)
            {
                User user = new User();
                // GENERATE QUERY
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT ID, NUM_ID, PASSWORD, DUMMY FROM USER ");
                sb.Append("WHERE ID=");
                sb.Append("'" + id + "'");
                string query = sb.ToString();

                DataSet ds = new DataSet();
                MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count != 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    user.Id = (string)row["id"];
                    user.Password = (string)row["password"];
                    object ul = (object) row["num_id"];
                    user.NumId = (int) Convert.ChangeType(ul, typeof(int));

                    if ( (bool) row[3] == true)
                    {
                        user.IsDummy = true;
                    }
                    else
                        user.IsDummy = false;
                }
                else
                {
                    return null;
                }

                return user;
            }// end lock 

        }// end method
    }
}
