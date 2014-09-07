using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    [Path("bounce")]
    [Order(8)]
    public class BounceProcessingSection: SectionBase
    {
        public bool BounceProcessing { get; set; }
        public double BounceScoreThreshold { get; set; }
        public ushort BounceInfoStaleAfter { get; set; }
        public ushort BounceYouAreDisabledWarnings { get; set; }
        public ushort BounceYouAreDisabledWarningsInterval { get; set; }
        public bool BounceUnrecognizedGoesToListOwner { get; set; }
        public bool BounceNotifyOwnerOnDisable { get; set; }
        public bool BounceNotifyOwnerOnRemoval { get; set; }

        public BounceProcessingSection(MailmanList list) : base(list) { }
    }
}
