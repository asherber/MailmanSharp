using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public class UnsubscribeResult
    {
        public IList<string> Unsubscribed { get; set; }
        public IList<string> NonMembers { get; set; }

        public UnsubscribeResult()
        {
            Unsubscribed = new List<string>();
            NonMembers = new List<string>();
        }
    }
}
