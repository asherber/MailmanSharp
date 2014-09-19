﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public enum AutorespondRequestsOption { No, YesWithDiscard, YesWithForward }

    [Path("autoreply")]
    [Order(11)]
    public class AutoResponderSection: SectionBase
    {
        public bool AutorespondPostings { get; set; }
        public List<string> AutoresponsePostingsText { get; set; }
        public bool AutorespondAdmin { get; set; }
        public List<string> AutoresponseAdminText { get; set; }
        public AutorespondRequestsOption AutorespondRequests { get; set; }
        public List<string> AutoresponseRequestText { get; set; }
        public ushort AutoresponseGraceperiod { get; set; }


        public AutoResponderSection(MailmanList list) : base(list) { }
    }
}
