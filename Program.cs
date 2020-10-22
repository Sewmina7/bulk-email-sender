using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;

namespace emailBot
{
    class Program
    {
        public static string tick = ((char)0x221A).ToString();
        public static ConsoleColor normalColor = ConsoleColor.Green;
        public static ConsoleColor warningColor = ConsoleColor.Yellow;
        public static ConsoleColor errorColor = ConsoleColor.Red;
        public static ConsoleColor importantColor = ConsoleColor.Magenta;
        public static ConsoleColor runningTaskColor = ConsoleColor.Cyan;

        public static string dataPath = AppDomain.CurrentDomain.BaseDirectory + "data/";
        static void Main(string[] args)
            
        {
            bool interrupt = false;
            fcolor(importantColor);
            Console.WriteLine("-----------------------------------");
            Console.Write("-------------");fcolor(normalColor); Console.Write("Welcome"); fcolor(importantColor); Console.WriteLine("---------------");
            Console.Write("--------------"); fcolor(normalColor); Console.Write("Email Bot"); fcolor(importantColor); Console.WriteLine("------------");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Thread.Sleep(400);
            fcolor(normalColor);
            Console.WriteLine("Press any key to continue...");
            if (interrupt)
            {
                Console.ReadKey();
            }
            Console.WriteLine();
            Console.WriteLine();

            fcolor(warningColor);
            Console.WriteLine("Checking data files at " + dataPath);
            fcolor(runningTaskColor);

            Settings settings;
            List<emailAddress> emails = new List<emailAddress>();
            emailData emailToSend;
            List<Campaign> campaigns = new List<Campaign>();
            string[] emailList;
            bool isHtml = false;
            if(!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
                interrupt = true;
            }
            try
            {
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(dataPath + "settings.json"));
                Console.WriteLine("settings " + tick);
            }
            catch
            {
                Console.Write("settings not found, creating... ");
                settings = new Settings();

                File.WriteAllText(dataPath + "settings.json", JsonConvert.SerializeObject(settings));
                Console.WriteLine(tick);
                interrupt = true;
            }
            try
            {                
                emails = JsonConvert.DeserializeObject<List<emailAddress>>(File.ReadAllText(dataPath+"emails.json"));
                Console.WriteLine("emails "+ tick);
            }
            catch
            {
                Console.Write("emails not found, creating... ");
                emails = new List<emailAddress>();
                emails.Add( new emailAddress("email1@mail.com", "password1"));
                emails.Add( new emailAddress("email2@mail.com", "password2"));

                File.WriteAllText(dataPath+"emails.json", JsonConvert.SerializeObject(emails));
                Console.WriteLine(tick);
                interrupt = true;
            }
            try
            {
                emailToSend = JsonConvert.DeserializeObject<emailData>(File.ReadAllText(dataPath + "emailToSend.json"));
                Console.WriteLine("email to send " + tick);
                emailToSend.Body = (emailToSend.Body.Contains("html")) ? File.ReadAllText(dataPath + emailToSend.Body) : emailToSend.Body;
                
                isHtml = emailToSend.Body.Contains("html");
            }
            catch
            {
                Console.Write("email to send not found, creating... ");
                emailToSend = new emailData("Hello", "How are you?");
                File.WriteAllText(dataPath + "emailToSend.json", JsonConvert.SerializeObject(emailToSend));
                Console.WriteLine(tick);
                interrupt = true;
            }

            try
            {
                Console.Write("Campaign History ");
   
                campaigns = JsonConvert.DeserializeObject<List<Campaign>>(File.ReadAllText(dataPath + "campaignHistory.json"));
                int totalMails = 0;
                foreach (Campaign campaign in campaigns)
                {
                    totalMails += campaign.sentAddresses.Count;
                }
                Console.WriteLine(" (" + totalMails + " total mails sent)"+ tick);
                
            }
            catch
            {
                Console.Write("Campaign History not found, creating... ");
                campaigns = new List<Campaign>();
                campaigns.Add(new Campaign(DateTime.Now.ToString(), "test1@gmail.com"));
                campaigns.Add(new Campaign((DateTime.Now - TimeSpan.FromSeconds(10)).ToString(),"test1@gmail.com"));
                campaigns[0].sentAddresses.Add("test1@gmail.com");
                campaigns[0].sentAddresses.Add("test23@gmail.com");
                
                File.WriteAllText(dataPath + "campaignHistory.json", JsonConvert.SerializeObject(campaigns));
                Console.WriteLine(tick);
                interrupt = true;
            }

            try
            {
                emailList = File.ReadAllLines(dataPath + "emailList.txt");
                Console.WriteLine("Email List " + tick);
            }
            catch
            {
                Console.Write("email list not found, creating empty list... ");
                emailList = new string[0];
                File.Create(dataPath + "emailList.txt");
                Console.WriteLine(tick);
                interrupt = true;
            }

            Console.WriteLine();
            Console.WriteLine();
            fcolor(importantColor);
            Console.WriteLine("All data files are configured well.. if you need to modify data files, modify them and reboot this program...");
            if (interrupt)
            {
                Console.WriteLine("Press any key to continue if you are happy with current data settings");
                Console.ReadKey();
            }

            fcolor(runningTaskColor);
            Console.WriteLine();
            Console.Write("Checking on emails  ");
            try
            {
                Campaign lastCampaign = Calc.getLatestCampaign(campaigns);
                double deltaHours = DateTime.Now.Subtract(DateTime.Parse(lastCampaign.timestamp)).TotalHours;

                Dictionary<string, int> msgsSent = Calc.usedMessages(campaigns, settings.emailsThresholdCooldownHours);

                List<string> receipents = Calc.getAvailableEmails(campaigns, emailList);
                List<MailMessage> mails = new List<MailMessage>();



                foreach (emailAddress email in emails)
                {
                    SmtpClient smtpClient = new SmtpClient(settings.smtpServer, settings.smtpPort);

                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                 //   smtpClient.Timeout = 20000;
                    smtpClient.Credentials = new System.Net.NetworkCredential(email.address, email.password);
                    smtpClient.EnableSsl = settings.enableSsl;
                    int msgsLeft = settings.emailsCountThreshold;
                    if (msgsSent.ContainsKey(email.address))
                    {
                        msgsLeft -= msgsSent[email.address];
                    }


                    while (msgsLeft > settings.maxReceipentsPerMsg && receipents.Count > settings.maxReceipentsPerMsg)
                    {
                        Campaign camp = new Campaign(DateTime.Now.ToString(), email.address);
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress(email.address);

                        camp.sentAddresses.Add(receipents[0]);
                        mail.To.Add(receipents[0]);
                        receipents.RemoveAt(0);

                        mail.Subject = emailToSend.Subject;
                        mail.Body = emailToSend.Body;
                        mail.IsBodyHtml = isHtml;
                        for (int i = 1; i < settings.maxReceipentsPerMsg; i++)
                        {
                            camp.sentAddresses.Add(receipents[i]);
                            mail.Bcc.Add(new MailAddress(receipents[i]));
                            receipents.RemoveAt(i);
                        }
                        Console.Write("Sending email with " + (mail.Bcc.Count+1) + " Receipents using " + email.address +"("+ msgsLeft+" msgs left)");
                        
                        smtpClient.Send(mail);
                        Console.WriteLine("    " + tick);
                        msgsLeft -= (mail.Bcc.Count + 1);
                        campaigns.Add(camp);

                        File.WriteAllText(dataPath + "campaignHistory.json", JsonConvert.SerializeObject(campaigns));
                    }
                    if (receipents.Count < settings.maxReceipentsPerMsg && receipents.Count > 0)
                    {
                        Campaign camp = new Campaign(DateTime.Now.ToString(), email.address);
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress(email.address);

                        mail.To.Add(receipents[0]);
                        receipents.RemoveAt(0);

                        mail.Subject = emailToSend.Subject;
                        mail.Body = emailToSend.Body;
                        mail.IsBodyHtml = isHtml;
                        if (msgsLeft <= receipents.Count)
                        {
                            for (int i = 1; i < msgsLeft; i++)
                            {
                                camp.sentAddresses.Add(receipents[i]);
                                mail.Bcc.Add(new MailAddress(receipents[i]));
                                receipents.RemoveAt(i);
                            }
                        }
                        else
                        {
                            for (int i = 1; i < receipents.Count; i++)
                            {
                                camp.sentAddresses.Add(receipents[i]);
                                mail.Bcc.Add(new MailAddress(receipents[i]));
                                receipents.RemoveAt(i);
                            }
                        }

                        Console.Write("Sending email with " + mail.Bcc.Count + " Receipents using " + email.address);
                        smtpClient.Send(mail);
                        campaigns.Add(camp);
                        msgsLeft -= (mail.Bcc.Count + 1);
                        File.WriteAllText(dataPath + "campaignHistory.json", JsonConvert.SerializeObject(campaigns));
                        Console.WriteLine("    " + tick);
                    }

                }
            }
            catch(Exception e)
            {

                Console.WriteLine("Something went worng :(");
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine("source : " + e.Source);
                Console.WriteLine("stack trace : " + e.StackTrace);
                Console.WriteLine("message : " + e.Message);
                Console.WriteLine("data : " + e.Data);

                Console.WriteLine();
                Console.ReadLine();
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Campaign done");
            Console.ReadLine();

            return;

            /*
            if (deltaHours > settings.emailsThresholdCooldownHours)
            {
                fcolor(runningTaskColor);
                Console.WriteLine("Launching campaign #" + (campaigns.Count + 1).ToString());
                fcolor(runningTaskColor);

                foreach(emailAddress email in emails)
                {
                    SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                    smtpClient.Credentials = new System.Net.NetworkCredential(email.address, email.password);
                    smtpClient.EnableSsl = true;

                    //List<string> toEmails = new List<string>();

                    for (int i = 0; i < receipents.Count; i++)
                    {
                        if( i > settings.emailsCountThreshold)
                        {
                            break;
                        }

                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress(email.address);
                        mail.To.Add(receipents[i]);


                        mail.Subject = emailToSend.Subject;
                        mail.Body = emailToSend.Body.Replace("{emailAdressHere}", receipents[i]);
                        mail.IsBodyHtml = isHtml;

                        Console.Write("Sending to " + receipents[i] + " with " + email.address);
                      
                       
                        smtpClient.Send(mail);
                        Console.WriteLine("    " + tick);
                       // toEmails.Add(emailsToSend[i]);
                        receipents.RemoveAt(i);
                    }

                }
            }
            else
            {
                fcolor(errorColor);
                Console.Write("Need to wait more ");fcolor(warningColor); Console.Write((settings.emailsThresholdCooldownHours - deltaHours).ToString()); fcolor(errorColor); Console.WriteLine(" hours for the next campaign");
            }

            Console.ReadKey();*/

        }


        static void fcolor(ConsoleColor c)
        {
            Console.ForegroundColor = c;
        }
    }
}
