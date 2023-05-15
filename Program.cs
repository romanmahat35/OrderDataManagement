using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace VHDU0050_出荷ﾃﾞｰﾀ取込
{
    class Program
    {
        static void Main(string[] args)
        {
            var filelocation = ConfigurationManager.AppSettings["InputFilePath"];
            Log.Trace("プログラムの開始...");
            DataAcess da = new DataAcess();
            Log.Trace("Start of data Extract");
            da.ExtractData(filelocation);
            Log.Trace("END of data Extract");
            Log.Trace("Start of data validadation");
            da.DataValidation();
            Log.Trace("End of data validadation");

            Log.Trace("Start of update");
            da.Update();
            Log.Trace("End of update");

            Console.Write("Press any key to continue");
            Console.ReadLine();
        }
        
    }
}
