using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System.IO;
using System.Diagnostics;

namespace VHDU0050_出荷ﾃﾞｰﾀ取込
{

    public class DataAcess
    {
        private OracleConnection conn = null;

        List<int> RECKBN = new List<int>();
        List<int> COOPKBN = new List<int>();
        List<int> SISYOCD = new List<int>();
        List<int> KUMICD = new List<int>();
        List<int> HATNO = new List<int>();
        List<int> CYUNO = new List<int>();
        List<int> SURYO = new List<int>();
        List<int> SKINGAKU = new List<int>();
        List<int> KKINGAKU = new List<int>();
        List<int> NOHINYMD = new List<int>();

        List<int> HANCD = new List<int>();
        List<int> HINKBN = new List<int>();
        List<int> HACHUYMD = new List<int>();

        List<int> HAISOYMD = new List<int>();
        List<int> tax = new List<int>();
        List<int> HONKYOTANNEW = new List<int>();
        List<int> CHUSO = new List<int>();

        public DataAcess()
        {
            var connectionstring = ConfigurationManager.ConnectionStrings["test"].ConnectionString;
            try
            {
                Log.Trace("データベース接続が開始されました...");
                conn = new OracleConnection(connectionstring);

                conn.Open();
                Log.Trace("データベースが接続されています。");
            }
            catch (Exception ex)
            {
                Log.Error($"{connectionstring} 存在しません。: {ex}");
            }

        }


        public void ExtractData(string filelocation)
        {
            Log.Trace("ファイルのロードを開始しています ...");
            if (!File.Exists(filelocation))
            {
                throw new FileNotFoundException("入力ファイルが指定されたパスに存在しません", filelocation);
            }



            string[] inputLines = File.ReadAllLines(filelocation);
          

            for (int i = 0; i < inputLines.Length; i++)
            {
                string line = inputLines[i];

                if (line[0] == '9')
                {
                    continue;
                }

                int reckbn = int.Parse(line.Substring(0, 1));
                int coopkbn = int.Parse(line.Substring(1, 1));
                int sisyocd = int.Parse(line.Substring(2, 3));
                int kumicd = int.Parse(line.Substring(5, 8));
                int hatno1 = int.Parse(line.Substring(13, 6));
                int cyuno1 = int.Parse(line.Substring(19, 6));
                int suryo1 = int.Parse(line.Substring(25, 2));
                int skingaku1 = int.Parse(line.Substring(27, 9));
                int kkingaku1 = int.Parse(line.Substring(36, 9));
                int nohinymd1 = int.Parse(line.Substring(45, 8));
                int hatno2 = int.Parse(line.Substring(53, 6));
                int cyuno2 = int.Parse(line.Substring(59, 6));
                int suryo2 = int.Parse(line.Substring(65, 2));
                int skingaku2 = int.Parse(line.Substring(67, 9));
                int kkingaku2 = int.Parse(line.Substring(76, 9));
                int nohinymd2 = int.Parse(line.Substring(85, 8));

                if (hatno1 != 0)
                {
                    //Debug.Assert(nohinymd1 != 0);
                    RECKBN.Add(reckbn);
                    COOPKBN.Add(coopkbn);
                    SISYOCD.Add(sisyocd);
                    KUMICD.Add(kumicd);
                    HATNO.Add(hatno1);
                    CYUNO.Add(cyuno1);
                    SURYO.Add(suryo1);
                    SKINGAKU.Add(skingaku1);
                    KKINGAKU.Add(kkingaku1);
                    NOHINYMD.Add(nohinymd1);
                }

                if (hatno2 != 0)
                {
                    //Debug.Assert(nohinymd2 != 0);
                    RECKBN.Add(reckbn);
                    COOPKBN.Add(coopkbn);
                    SISYOCD.Add(sisyocd);
                    KUMICD.Add(kumicd);
                    HATNO.Add(hatno2);
                    CYUNO.Add(cyuno2);
                    SURYO.Add(suryo2);
                    SKINGAKU.Add(skingaku2);
                    KKINGAKU.Add(kkingaku2);
                    NOHINYMD.Add(nohinymd2);
                }
            }


        }


        private static DateTime DateTimeFromNumber(int n)
        {
            // YYYYMMDD
            int DD = n % 100;
            int MM = (n / 100) % 100;
            int YYYY = (n / 10000);
            return new DateTime(YYYY, MM, DD);
        }

        public void taxcalculation()
        {
            var Taxrate1 = int.Parse(ConfigurationManager.AppSettings["Taxrate1"]);
            var Taxrate2 = int.Parse(ConfigurationManager.AppSettings["Taxrate2"]);
            var Taxrate3 = int.Parse(ConfigurationManager.AppSettings["Taxrate3"]);
            var Taxdate1 = int.Parse(ConfigurationManager.AppSettings["Taxdate1"]);
            var Taxdate2 = int.Parse(ConfigurationManager.AppSettings["Taxdate2"]);

            for (int i = 0; i < COOPKBN.Count; i++)
            {
                var deliverydate = HAISOYMD[i];
                if (deliverydate < Taxdate1)
                {
                    tax.Add(Taxrate1);
                }
                else if (deliverydate < Taxdate2)
                {
                    tax.Add(Taxrate2);
                }
                else
                {
                    tax.Add(Taxrate3);
                }

                if (HACHUYMD[i] < Taxdate2 && deliverydate >= Taxdate2)
                {
                    var HonkyotanNew = -1 * (int)(-1 * KKINGAKU[i] / (float)SURYO[i] / (1 + tax[i] * 0.01));
                    var BTANKA2 = (int)(HonkyotanNew * (1 + tax[i] * 0.01));
                    if (KKINGAKU[i] / (float)SURYO[i] < BTANKA2)
                    {
                        HonkyotanNew = HonkyotanNew - 1;
                    }
                    HONKYOTANNEW.Add(HonkyotanNew);
                }
                else
                {
                    HONKYOTANNEW.Add(-1);
                }
            }

        }

        public void weekdaylogic()
        {
            for (int i = 0; i < COOPKBN.Count; ++i)
            {
                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT SUBSTR( HCRS, 1, 1 ) AS weekday FROM test WHERE COOPKBN = {COOPKBN[i]} AND HANCD = {HANCD[i]}";

                    using (var reader = cmd.ExecuteReader())
                    {

                        if (!reader.Read())
                            throw new InvalidDataException("invalid data");

                        int youbi = (int)(reader.GetString(0)[0] - '0');

                        if (youbi > 0 && youbi < 8 && HINKBN[i] == 0)
                        {
                            DateTime strHAISOYMD;
                            var date = DateTimeFromNumber(NOHINYMD[i]);

                            var youbi_text = (int)date.DayOfWeek;
                            if (youbi == youbi_text)
                            {
                                strHAISOYMD = date.AddDays(7);
                            }
                            else if (youbi > youbi_text)
                            {
                                strHAISOYMD = date.AddDays(youbi - youbi_text);
                            }
                            else
                            {
                                strHAISOYMD = date.AddDays(youbi + 7 - youbi_text);
                            }
                            HAISOYMD.Add(int.Parse(strHAISOYMD.ToString("yyyyMMdd")));
                        }
                        else
                        {
                            HAISOYMD.Add(NOHINYMD[i]);
                        }
                    }

                }
            }
        }

     
       

        public void Update()
        {

            var KOSINCOOPKBNConst = int.Parse(ConfigurationManager.AppSettings["KOSINCOOPKBN"]);
            var KOSINSISYOCDConst = int.Parse(ConfigurationManager.AppSettings["KOSINSISYOCD"]);
            var KOSINIDConst = int.Parse(ConfigurationManager.AppSettings["KOSINID"]);

            weekdaylogic();
            taxcalculation();
            OracleCommand cmd = conn.CreateCommand();
            var cmdQuery = @"UPDATE  test
                                   SET  SYUKAF = :RECKBN,
                                        KKINGAKU = CASE WHEN :RECKBN1 = 3 THEN :KKINGAKU END,
                                        BTANKA = CASE WHEN :RECKBN2 = 3  THEN :KKINGAKU1/CHUSU END,
                                        NOHINYMD = CASE WHEN :RECKBN3 = 3  THEN :NOHINYMD ELSE 0 END,
                                        HAISOYMD = CASE WHEN :RECKBN4 = 3  THEN :HAISOYMD ELSE 0 END,
                                        SELECTFLG = CASE WHEN :RECKBN5 = 3 OR :RECKBN6 = 4 THEN 0 END,
                                        NTANKA = (:SKINGAKU *100)/CHUSU/100,
                                        SKINGAKU = :SKINGAKU1,
                                        KOSINCNT = decode(KOSINCNT,99999,1,KOSINCNT+1),
                                        KOSINCOOPKBN = {0},
                                        KOSINSISYOCD = {1},
                                        KOSINID = {2},
                                        KOSINYMD = TO_CHAR(SYSDATE,'DDMMYYYY'),
                                        KOSINTIME = TO_CHAR(SYSDATE,'HH24MISS'),
                                        HONKYOTAN = CASE WHEN :RECKBN7 = 3 AND :HONKYOTAN >= 0 then :HONKYOTAN1 END ,
                                        ZEIRITU  = CASE WHEN :RECKBN8 = 3 AND :HONKYOTAN2 >= 0 then :tax END
                                        WHERE HATNO = :HATNO AND KUMICD = :KUMICD AND COOPKBN = :COOPKBN";

            cmd.CommandText = string.Format(cmdQuery, KOSINCOOPKBNConst, KOSINSISYOCDConst, KOSINIDConst);
            cmd.CommandType = CommandType.Text;

            cmd.ArrayBindCount = HINKBN.Count;

            OracleParameter RECKBN = new OracleParameter();
            RECKBN.OracleDbType = OracleDbType.Int32;
            RECKBN.Value = this.RECKBN.ToArray();
            RECKBN.ParameterName = "RECKBN1";
            cmd.Parameters.Add(RECKBN);

            OracleParameter RECKBN1 = new OracleParameter();
            RECKBN1.OracleDbType = OracleDbType.Int32;
            RECKBN1.Value = this.RECKBN.ToArray();
            RECKBN1.ParameterName = "RECKBN1";
            cmd.Parameters.Add(RECKBN1);

            OracleParameter RECKBN2 = new OracleParameter();
            RECKBN2.OracleDbType = OracleDbType.Int32;
            RECKBN2.Value = this.RECKBN.ToArray();
            RECKBN2.ParameterName = "RECKBN1";
            cmd.Parameters.Add(RECKBN2);

            OracleParameter RECKBN3 = new OracleParameter();
            RECKBN3.OracleDbType = OracleDbType.Int32;
            RECKBN3.Value = this.RECKBN.ToArray();
            RECKBN3.ParameterName = "RECKBN3";
            cmd.Parameters.Add(RECKBN3);

            OracleParameter RECKBN4 = new OracleParameter();
            RECKBN4.OracleDbType = OracleDbType.Int32;
            RECKBN4.Value = this.RECKBN.ToArray();
            RECKBN4.ParameterName = "RECKBN4";
            cmd.Parameters.Add(RECKBN4);

            OracleParameter RECKBN5 = new OracleParameter();
            RECKBN5.OracleDbType = OracleDbType.Int32;
            RECKBN5.Value = this.RECKBN.ToArray();
            RECKBN5.ParameterName = "RECKBN5";
            cmd.Parameters.Add(RECKBN5);

            OracleParameter RECKBN6 = new OracleParameter();
            RECKBN6.OracleDbType = OracleDbType.Int32;
            RECKBN6.Value = this.RECKBN.ToArray();
            RECKBN6.ParameterName = "RECKBN6";
            cmd.Parameters.Add(RECKBN6);

            OracleParameter RECKBN7 = new OracleParameter();
            RECKBN7.OracleDbType = OracleDbType.Int32;
            RECKBN7.Value = this.RECKBN.ToArray();
            RECKBN7.ParameterName = "RECKBN7";
            cmd.Parameters.Add(RECKBN7);

            OracleParameter RECKBN8 = new OracleParameter();
            RECKBN8.OracleDbType = OracleDbType.Int32;
            RECKBN8.Value = this.RECKBN.ToArray();
            RECKBN8.ParameterName = "RECKBN8";
            cmd.Parameters.Add(RECKBN8);

            OracleParameter KKINGAKU = new OracleParameter();
            KKINGAKU.OracleDbType = OracleDbType.Int32;
            KKINGAKU.Value = this.KKINGAKU.ToArray();
            KKINGAKU.ParameterName = "KKINGAKU";
            cmd.Parameters.Add(KKINGAKU);

            OracleParameter KKINGAKU1 = new OracleParameter();
            KKINGAKU1.OracleDbType = OracleDbType.Int32;
            KKINGAKU1.Value = this.KKINGAKU.ToArray();
            KKINGAKU1.ParameterName = "KKINGAKU1";
            cmd.Parameters.Add(KKINGAKU1);



            OracleParameter NOHINYMD = new OracleParameter();
            NOHINYMD.OracleDbType = OracleDbType.Int32;
            NOHINYMD.Value = this.NOHINYMD.ToArray();
            NOHINYMD.ParameterName = "NOHINYMD";
            cmd.Parameters.Add(NOHINYMD);


            OracleParameter HAISOYMD = new OracleParameter();
            HAISOYMD.OracleDbType = OracleDbType.Int32;
            HAISOYMD.Value = this.HAISOYMD.ToArray();
            HAISOYMD.ParameterName = "HAISOYMD";
            cmd.Parameters.Add(HAISOYMD);

            OracleParameter SKINGAKU = new OracleParameter();
            SKINGAKU.OracleDbType = OracleDbType.Int32;
            SKINGAKU.Value = this.SKINGAKU.ToArray();
            SKINGAKU.ParameterName = "SKINGAKU";
            cmd.Parameters.Add(SKINGAKU);

            OracleParameter SKINGAKU1 = new OracleParameter();
            SKINGAKU1.OracleDbType = OracleDbType.Int32;
            SKINGAKU1.Value = this.SKINGAKU.ToArray();
            SKINGAKU1.ParameterName = "SKINGAKU1";
            cmd.Parameters.Add(SKINGAKU1);


            OracleParameter HATNO = new OracleParameter();
            HATNO.OracleDbType = OracleDbType.Int32;
            HATNO.Value = this.HATNO.ToArray();
            HATNO.ParameterName = "HATNO";
            cmd.Parameters.Add(HATNO);


            OracleParameter KUMICD = new OracleParameter();
            KUMICD.OracleDbType = OracleDbType.Int32;
            KUMICD.Value = this.KUMICD.ToArray();
            KUMICD.ParameterName = "KUMICD";
            cmd.Parameters.Add(KUMICD);




            OracleParameter COOPKBN = new OracleParameter();
            COOPKBN.OracleDbType = OracleDbType.Int32;
            COOPKBN.Value = this.COOPKBN.ToArray();
            COOPKBN.ParameterName = "COOPKBN";
            cmd.Parameters.Add(COOPKBN);


            OracleParameter HONKYOTAN = new OracleParameter();
            HONKYOTAN.OracleDbType = OracleDbType.Int32;
            HONKYOTAN.Value = this.HONKYOTANNEW.ToArray();
            HONKYOTAN.ParameterName = "HONKYOTAN";
            cmd.Parameters.Add(HONKYOTAN);

            OracleParameter HONKYOTAN1 = new OracleParameter();
            HONKYOTAN1.OracleDbType = OracleDbType.Int32;
            HONKYOTAN1.Value = this.HONKYOTANNEW.ToArray();
            HONKYOTAN1.ParameterName = "HONKYOTAN1";
            cmd.Parameters.Add(HONKYOTAN1);

            OracleParameter HONKYOTAN2 = new OracleParameter();
            HONKYOTAN2.OracleDbType = OracleDbType.Int32;
            HONKYOTAN2.Value = this.HONKYOTANNEW.ToArray();
            HONKYOTAN2.ParameterName = "HONKYOTAN2";
            cmd.Parameters.Add(HONKYOTAN2);


            OracleParameter tax = new OracleParameter();
            tax.OracleDbType = OracleDbType.Int32;
            tax.Value = this.tax.ToArray();
            tax.ParameterName = "tax";
            cmd.Parameters.Add(tax);


            cmd.ExecuteNonQuery();

        }

        public void CheckHATNO(DataTable dataTable)
        {
            for (int i = 0; i < COOPKBN.Count; ++i)
            {
                if (HATNO[i] == 0)
                {
                    continue;
                }
                var results = from DataRow row in dataTable.Rows
                              where
                              (Int32)row["KUMICD"] == KUMICD[i] &&
                              (Int16)row["COOPKBN"] == COOPKBN[i] &&
                             (Int32)row["HATNO"] == HATNO[i]
                              select new { CHUSO = row["CHUSU"], HINKBN = row["HINKBN"], HACHUYMD = row["HACHUYMD"] };

                if (!results.Any())
                {
                    throw new InvalidDataException("Data Not found");
                }

                HINKBN.Add((Int16)results.First().HINKBN);
                HACHUYMD.Add((int)results.First().HACHUYMD);
                CHUSO.Add((Int16)results.First().CHUSO);
            }
        }

        public void DataValidation()
        {
            foreach (var record in RECKBN)
            {
                if (record < 3 || record > 6)
                {
                    throw new InvalidDataException("bad record");
                }
            }

            Log.Warning("starting downloading test");


            DataTable KMTR01FL01 = new DataTable();
            string query = "SELECT HINKBN, CHUSU,HATNO,KUMICD,COOPKBN,HACHUYMD FROM test ";
            var cmd = new OracleCommand(query, conn);
            OracleDataAdapter da = new OracleDataAdapter(cmd);
            da.Fill(KMTR01FL01);
            Log.Warning("ending downloading test");
            Log.Warning("starting hatno");
            CheckHATNO(KMTR01FL01);

            Log.Warning("ending hatno");


            Log.Warning("starting downloading KKKM01MST99S");
            DataTable KKKM01MST99S = new DataTable();
            string KKKM01MST99S_query = "SELECT  KBN,COOPKBN FROM test WHERE KBN='1'";
            var cmd1 = new OracleCommand(KKKM01MST99S_query, conn);
            OracleDataAdapter da1 = new OracleDataAdapter(cmd1);
            da1.Fill(KKKM01MST99S);
            Log.Warning("end downloading KKKM01MST99S");

            for (int i = 0; i < COOPKBN.Count; ++i)
            {
                var result = from DataRow row in KKKM01MST99S.Rows
                             where (Int16)row["COOPKBN"] == COOPKBN[i]
                             select row["COOPKBN"];

                if (!result.Any())
                    throw new InvalidDataException();
            }


            Log.Warning("starting downloading test");
            DataTable MF_ZIGYOSYO = new DataTable();
            string MF_ZIGYOSYO_query = "SELECT  SISYOCD FROM test ";
            var cmd2 = new OracleCommand(MF_ZIGYOSYO_query, conn);
            OracleDataAdapter da2 = new OracleDataAdapter(cmd2);
            da2.Fill(MF_ZIGYOSYO);

            Log.Warning("end downloading test");

            for (int i = 0; i < SISYOCD.Count; ++i)
            {
                var result = from DataRow row in MF_ZIGYOSYO.Rows
                             where (Int16)row["SISYOCD"] == SISYOCD[i]
                             select row["SISYOCD"];

                if (!result.Any())
                    throw new InvalidDataException();

            }

            Log.Warning("starting downloading test");
            DataTable MF_KUMIAIIN = new DataTable();
            DataTable FS_KUMIKOUSIN = new DataTable();
            string MF_KUMIAIIN_query = "SELECT HHANCD,COOPKBN,KUMICD FROM test";
            string FS_KUMIKOUSIN_query = "SELECT HANCD,COOPKBN,KOJINCD  FROM test";
            var cmd3 = new OracleCommand(MF_KUMIAIIN_query, conn);
            var cmd4 = new OracleCommand(FS_KUMIKOUSIN_query, conn);
            OracleDataAdapter da3 = new OracleDataAdapter(cmd3);
            OracleDataAdapter da4 = new OracleDataAdapter(cmd4);
            da3.Fill(MF_KUMIAIIN);
            da4.Fill(FS_KUMIKOUSIN);
            Log.Warning("end downloading test");

            for (int i = 0; i < COOPKBN.Count; i++)
            {
                var result = from DataRow row in MF_KUMIAIIN.Rows
                             where (Int16)row["COOPKBN"] == COOPKBN[i] && (Int32)row["KUMICD"] == KUMICD[i]
                             select row["HHANCD"];
                if (!result.Any())
                {
                    result = from DataRow row in FS_KUMIKOUSIN.Rows
                             where (Int16)row["COOPKBN"] == COOPKBN[i] && (Int32)row["KOJINCD"] == KUMICD[i]
                             select row["HANCD"];
                }

                if (!result.Any())
                {
                    throw new InvalidDataException("Data not found");
                }

                HANCD.Add((int)result.First());
            }

        }
    }
}