using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    public enum PersonalizeOption { No, Yes, FullPersonalization }
    
    [Path("nondigest")]
    public class NonDigestSection: SectionBase
    {
        public bool Nondigestable { get; set; }
        public PersonalizeOption Personalize { get; set; }
        public string MsgHeader { get; set; }
        public string MsgFooter { get; set; }
        public bool ScrubNondigest { get; set; }
        public string RegularExcludeLists { get; set; }
        public bool RegularExcludeIgnore { get; set; }
        public string RegularIncludeLists { get; set; }

        public NonDigestSection(MailmanList list) : base(list) { }
    }
}
