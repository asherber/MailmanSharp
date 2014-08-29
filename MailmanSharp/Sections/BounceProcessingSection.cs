using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    [Path("bounce")]
    public class BounceProcessingSection: SectionBase
    {
        public bool BounceProcessing { get; set; }
        //TODO: This should be float
        public ushort BounceScoreThreshold { get; set; }
        public ushort BounceInfoStaleAfter { get; set; }
        public ushort BounceYouAreDisabledWarnings { get; set; }
        public ushort BounceYouAreDisabledWarningInterval { get; set; }
        public bool BounceUnrecognizedGoesToListOwner { get; set; }
        public bool BounceNotifyOwnerOnDisable { get; set; }
        public bool BounceNotifyOwnerOnRemoval { get; set; }

        public BounceProcessingSection(MailmanList list) : base(list) { }
    }
}
