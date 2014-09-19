using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public enum PersonalizeOption { No, Yes, FullPersonalization }
    
    [Path("nondigest")]
    [Order(5)]
    public class NonDigestSection: SectionBase
    {
        public bool Nondigestable { get; set; }
        public PersonalizeOption Personalize { get; set; }
        public List<string> MsgHeader { get; set; }
        public List<string> MsgFooter { get; set; }
        public bool ScrubNondigest { get; set; }
        public List<string> RegularExcludeLists { get; set; }
        public bool RegularExcludeIgnore { get; set; }
        public List<string> RegularIncludeLists { get; set; }

        public NonDigestSection(MailmanList list) : base(list) { }
    }
}
