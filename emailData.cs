using System;
using System.Collections.Generic;
using System.Text;

namespace emailBot
{
    [Serializable]
    public class emailAddress
    {
        public string address { get; set; }
        public string password { get; set; }

        public emailAddress(string _email, string _pass)
        {
           address = _email;
            password = _pass;
        }
    }


    [Serializable]
    public class emailData
    {
        public string Subject { get; set; }
        public string Body { get; set; }

        public emailData(string subject, string body)
        {
            Subject = subject;
            Body = body;
        }
    }

    [Serializable]
    public class Settings
    {
        public string smtpServer { get; set; }
        public int smtpPort { get; set; }
        public bool enableSsl { get; set; }
        public int emailsCountThreshold { get; set; }
        public int emailsThresholdCooldownHours { get; set; }
        public int maxReceipentsPerMsg { get; set; }
        public Settings()
        {
            enableSsl = true;
            smtpServer = "example.smtp.com";
            smtpPort = 543;
            emailsCountThreshold = 450;
            emailsThresholdCooldownHours = 24;
            maxReceipentsPerMsg = 100;            
        }
    }

    [Serializable]
    public class Campaign
    {
        public string usedEmail { get; set; }
        public string timestamp { get; set; }
        public List<string> sentAddresses { get; set; }

        public Campaign (string startedTime, string _usedEmail)
        {
            timestamp = startedTime;
            usedEmail = _usedEmail;
            sentAddresses = new List<string>();
        }
        
    }
}
