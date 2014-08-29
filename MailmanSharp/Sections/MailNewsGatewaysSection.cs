using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    public enum NewsModerationOption { None, OpenListModeratedGroup, Moderated }

    [Path("gateway")]
    public class MailNewsGatewaysSection: SectionBase
    {
        public string NntpHost { get; set; }
        public string LinkedNewsgroup { get; set; }
        public bool GatewayToNews { get; set; }
        public bool GatewayToMail { get; set; }
        private NewsModerationOption NewsModeration { get; set; }
        public bool NewPrefixSubjectToo { get; set; }
        public bool _MassCatchup { get; set; }

        public MailNewsGatewaysSection(MailmanList list) : base(list) { }
    }
}
