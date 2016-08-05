using System;
using LunkerRedis.src;
using log4net;
using LunkerRedis.src.Common;
using LunkerRedis.src.Utils;
using System.Xml;

namespace LunkerRedis
{
    class Program
    {
        private static ILog logger = FileLogger.GetLoggerInstance();
        private static Backend backendServer = null;
        private static bool appState = MyConst.Run;

        static void Main(string[] args)
        {
            /*
            XmlTextReader reader = new XmlTextReader("config\\MySQLConfig.xml");
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.Name.Equals("MySQLConfig"))
                            continue;
                        Console.Write("<" + reader.Name);
                        Console.WriteLine(">");
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        Console.WriteLine(reader.Name);
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (reader.Name.Equals("MySQLConfig"))
                            continue;
                        Console.Write("</" + reader.Name);
                        Console.WriteLine(">");
                        break;
                }
            }
            */


            XmlTextReader reader = new XmlTextReader("config\\RedisConfig.xml");
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
            string config = ip + ":" + port + ",allowAdmin=true";
            logger.Debug(config);
            logger.Debug("\n\n\n\n\n--------------------------------------------Backend Server-----------------------------------------------------");
            logger.Debug("--------------------------------------------Start Program-----------------------------------------------------");



            backendServer = new Backend();
            backendServer.Start();

            while (appState)
            {
                Console.Write("어플리케이션을 종료하시겠습니까? (y/n) : ");
                string close = Console.ReadLine();
                if (close.Equals("y") || close.Equals("Y"))
                {
                    backendServer.RequestStopThread();
                    RedisClientPool.GetInstance().ClearDB();
                    RedisClientPool.Dispose();
                    MySQLClientPool.Dispose();
                    appState = MyConst.Exit;

                    Console.Clear();
                    Console.Write("어플리케이션을 종료중입니다 . . .");
                    logger.Debug("--------------------------------------------Exit Program-----------------------------------------------------");
                    Environment.Exit(0);
                    
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("다시 입력하십시오.");
                }
            }
        }// end method
    }
}