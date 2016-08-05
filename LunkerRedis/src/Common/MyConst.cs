using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LunkerRedis.src.Common
{
    public static class MyConst
    {
        static MyConst()
        {
            // Read MySQL Config
            StringBuilder sb = new StringBuilder();

            XmlTextReader reader = new XmlTextReader("config\\MySQLConfig.xml");
            while (reader.Read())
            {

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.Name.Equals("MySQLConfig"))
                            continue;
                        sb.Append(reader.Name);
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        sb.Append("=");
                        sb.Append(reader.Value);
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (reader.Name.Equals("MySQLConfig"))
                            continue;
                        sb.Append(";");
                        break;
                }
            }
            mysqlConfig = sb.ToString();

            /*
             * read redis config 
             */
            reader = new XmlTextReader("config\\RedisConfig.xml");
            int index = 0;
            string ip = "";
            int port = 0;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.Name.Equals("RedisConfig"))
                            continue;
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        //sb.Append(reader.Value);
                        if (index == 0)
                            ip = reader.Value;
                        else
                            port = int.Parse(reader.Value);
                        index++;
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (reader.Name.Equals("RedisConfig"))
                            continue;
                        break;
                }
            }// end while 
            redisConfig = ip + ":" + port + ",allowAdmin=true";

            // read App Config 
            reader = new XmlTextReader("config\\Appconfig.xml");
            
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.Name.Equals("AppConfig"))
                            continue;
                        else if (reader.Name.Equals("frontendListenPort"))
                        {
                            reader.Read();
                            frontendPort = int.Parse(reader.Value);
                        }
                        else if (reader.Name.Equals("clientListenPort"))
                        {
                            reader.Read();
                            clientPort = int.Parse(reader.Value);
                        }
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        //sb.Append(reader.Value);
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (reader.Name.Equals("Appconfig"))
                            continue;
                        break;
                }
            }// end while 
        }// end static structor

        public static string mysqlConfig = "";
        public static string redisConfig = "";

        public static int frontendPort = 25389;
        public static int clientPort = 20852;
      
        public static int HEADER_LENGTH = 4;

        public static bool LOGINED = true;
        public static bool LOGOUT = false;

        public static bool Dummy = true;
        public static bool User = false;

        public static string LoggerConfigPath = "config\\Logconfig.xml";
        public static string Logger = "Logger";

        public static bool Run = true;
        public static bool Exit = false;
    }
}
