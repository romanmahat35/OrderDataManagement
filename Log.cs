using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHDU0050_出荷ﾃﾞｰﾀ取込
{
    static class Log
    {

        private static StreamWriter LogFile;

        private static void WriteToFile(string str)
        {
            var Logfilelocation = ConfigurationManager.AppSettings["LogFilePath"];
            if (LogFile == null)
            {
                LogFile = new StreamWriter(new FileStream(Logfilelocation + DateTime.Now.ToString("yyyyMMdd") + ".log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            }
            LogFile.WriteLine(str);
            LogFile.Flush();
        }

        public static void Fatal(string fmt, params object[] objs)
        {
            Error(fmt, objs);
            Environment.Exit(1);
        }
        public static void Error(string fmt, params object[] objs)
        {
            var str = string.Format(fmt, objs);
            str = $"{DateTime.Now.ToString()} : [エラー] : {str}";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ResetColor();
            WriteToFile(str);
        }

        public static void Warning(string fmt, params object[] objs)
        {
            var str = string.Format(fmt, objs);
            str = $"{DateTime.Now.ToString()} : [警告] : {str}";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(str);
            Console.ResetColor();
            WriteToFile(str);
        }

        public static void Trace(string fmt, params object[] objs)
        {
            var str = string.Format(fmt, objs);
            str = $"{DateTime.Now.ToString()} : [情報] : {str}";
            Console.WriteLine(str);
            WriteToFile(str);
        }

        public static void printline()
        {
            var line = "------------------------------------------------------------------------------------------------------------------";
            Console.WriteLine(line);
            WriteToFile(line);
        }


    }
}
