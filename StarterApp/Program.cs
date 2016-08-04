using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Timers;
namespace StarterApp
{
    class Program
    {

        private static Process mainProcess = null;
        private static ProcessStartInfo info = null;

        static void Main(string[] args)
        {
            info = new ProcessStartInfo();
            info.FileName = "D:\\workspace\\LunkerRedis\\LunkerRedis\\bin\\Debug\\LunkerRedis.exe";
            info.CreateNoWindow = false;
            
            mainProcess = Process.Start(info);
            //mainProcess = Process.GetProcessesByName("LunkerRedis")[0];

            Timer timer = new System.Timers.Timer();
            timer.Interval = 5 * 1000; // 1 시간
            timer.Elapsed += new ElapsedEventHandler(CheckProcess);
            

            timer.Start();

            Console.ReadKey();
        }// end method
        
        public static void CheckProcess(object sender, ElapsedEventArgs e)
        {
            //Console.WriteLine("타이머 똑딱");
            if (mainProcess.HasExited)
            {
                mainProcess.Dispose();
                Console.WriteLine("Main프로세스가 죽었음");

                Console.WriteLine("Main프로세스 재시작!!!!!!");

                mainProcess = Process.Start(info);

                Console.WriteLine("Main의 프로세스 아이디: " + mainProcess.Id);
                Console.WriteLine("Main프로세스가 재시작 성공");

            }
        }
    }

}


