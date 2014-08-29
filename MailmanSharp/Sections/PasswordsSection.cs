using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    public class PasswordsSection: SectionBase
    {
        public string Administrator { get; set; }
        public string Moderator { get; set; }
        public string Poster { get; set; }

        public PasswordsSection(MailmanList list) : base(list) { }
    }
}
