using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Core
{
    public class ChromeBrowser : IBrowser
    {
        public string Name { get; set; }
        public string MinVersion { get; set; }
        public string HistoryPath { get; set; }
        public List<HistoryElement> History { get; set; }

        static readonly DateTime epoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        string ConnectionString
        {
            get
            {
                return $"Data Source={HistoryPath}";
            }
        }
        string ExecupteCommand = "Select * From urls";

        string original = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Google\Chrome\User Data\Default\History";
        string copy = $@"{Environment.CurrentDirectory}\temp\History";

        public ChromeBrowser()
        {
            Name = "Chrome";
            MinVersion = "62.0.3202.94";
            HistoryPath = original;
            History = new List<HistoryElement>();
        }

        public void UpdateHistory()
        {
            try
            {
                Update();
            }
            catch (SQLiteException e)
            {
                if (e.ErrorCode == 5)
                {
                    HistoryPath = copy;

                    SaveCopy();
                    Update();
                    File.Delete(copy);

                    HistoryPath = original;
                }
            }
        }
        public static DateTime ConvertUnixTimeStamp(string unixTimeStamp)
        {
            long.TryParse(unixTimeStamp, out long l);
            return DateTime.FromBinary(l).AddYears(1975).AddMonths(3).AddHours(2).AddMinutes(40);
        }

        void SaveCopy()
        {
            if (!Directory.Exists($@"{Environment.CurrentDirectory}\temp\"))
                Directory.CreateDirectory($@"{Environment.CurrentDirectory}\temp\");

            File.Copy(original, copy, true);
        }
        void Update()
        {
            History.Clear();

            using (SQLiteConnection c = new SQLiteConnection(ConnectionString))
            {
                c.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(ExecupteCommand, c))
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        History.Add(new HistoryElement()
                        {
                            Date = ConvertUnixTimeStamp(rdr[5].ToString()),
                            Url = rdr[1].ToString(),
                            Title = rdr[2].ToString(),
                            VisitCount = Convert.ToInt32(rdr[3])
                        });
                    }
                }
            }
        }
    }
}
