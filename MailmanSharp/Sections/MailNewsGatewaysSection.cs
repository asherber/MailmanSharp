using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public enum NewsModerationOption { None, OpenListModeratedGroup, Moderated }

    [Path("gateway")]
    [Order(10)]
    public class MailNewsGatewaysSection: SectionBase
    {
        public string NntpHost { get; set; }
        public string LinkedNewsgroup { get; set; }
        public bool GatewayToNews { get; set; }
        public bool GatewayToMail { get; set; }
        private NewsModerationOption NewsModeration { get; set; }
        public bool NewPrefixSubjectToo { get; set; }
        
        public void MassCatchup()
        {
            this.Client.PostAdminRequest(_paths.First(), "_mass_catchup", 1);
        }

        public MailNewsGatewaysSection(MailmanList list) : base(list) { }
    }
}
