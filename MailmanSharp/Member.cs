using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public class Member
    {
        public string Email { get; set; }
        public string RealName { get; set; }
        public bool Mod { get; set; }
        public bool Hide { get; set; }
        public bool NoMail { get; set; }
        public bool Ack { get; set; }
        public bool NotMeToo { get; set; }
        public bool NoDupes { get; set; }
        public bool Digest { get; set; }
        public bool Plain { get; set; }

        public Member()
        {
            this.NoDupes = true;
            this.Plain = true;
        }

        public Member(string email, string realName = "")
        {
            this.Email = email;
            this.RealName = realName;
        }
    }
}
