using System;
using System.Collections.Generic;
using System.Text;

namespace emailBot
{
    public static class Calc
    {

        public static Campaign getLatestCampaign(List<Campaign> campaigns)
        {
            Campaign lastCampaign = null;
            foreach (Campaign campaign in campaigns)
            {
                if (lastCampaign == null)
                {
                    lastCampaign = campaign;
                    continue;
                }

                if (DateTime.Parse(lastCampaign.timestamp).CompareTo(DateTime.Parse(campaign.timestamp)) > 0)
                {
                    lastCampaign = campaign;
                }
            }

            return lastCampaign;
        }

        public static List<string> getAvailableEmails(List<Campaign> campaigns, string[] emailList) {
            List<string> availableMails = new List<string>();

            foreach(string email in emailList)
            {
                if(!checkIfSent(campaigns, email))
                {
                    availableMails.Add(email);
                }
            }

            return availableMails;
                
        }

        public static bool checkIfSent(List<Campaign> campaigns, string email)
        {
            foreach (Campaign campaign in campaigns)
            {
                foreach (string address in campaign.sentAddresses)
                {
                    if (email == address)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static Dictionary<string, int> usedMessages(List<Campaign> campaigns, int cooldownTime)
        {
            Dictionary<string, int> _usedMsgs = new Dictionary<string, int>();

            foreach(Campaign campaign in campaigns)
            {
                if(DateTime.Now.Subtract(DateTime.Parse(campaign.timestamp)).TotalHours > cooldownTime)
                {
                    continue;
                }

                if (_usedMsgs.ContainsKey(campaign.usedEmail))
                {
                    _usedMsgs[campaign.usedEmail] += campaign.sentAddresses.Count;
                }
                else
                {
                    _usedMsgs.Add(campaign.usedEmail, campaign.sentAddresses.Count);
                }
            }

            return _usedMsgs;
        }

    }
}
