using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public class SubscribeResult
    {
        public IList<string> Subscribed { get; set; }
        public IList<string> AlreadyMembers { get; set; }
        public IList<string> BadEmails { get; set; }

        public SubscribeResult()
        {
            Subscribed = new List<string>();
            AlreadyMembers = new List<string>();
            BadEmails = new List<string>();
        }
    }
}
