using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    public enum FromIsListOption { No, MungFrom, WrapMessage }
    public enum ReplyGoesToListOption { Poster, ThisList, ExplicitAddress }
    [Flags]
    public enum NewMemberOptions { None = 0, Hide = 1, Ack = 2, NotMeToo = 4, NoDupes = 8 }

    [Path("general")]
    public class GeneralSection: SectionBase
    {
        public string RealName { get; set; }
        public List<string> Owner { get; set; }
        public List<string> Moderator { get; set; }
        public string Description { get; set; }
        public List<string> Info { get; set; }
        public string SubjectPrefix { get; set; }
        public FromIsListOption FromIsList { get; set; }
        public bool AnonymousList { get; set; }
        public bool FirstStripReplyTo { get; set; }
        public ReplyGoesToListOption ReplyGoesToList { get; set; }
        public string ReplyToAddress { get; set; }
        public bool UmbrellaList { get; set; }
        public string UmbrellaMemberSuffix { get; set; }
        public bool SendReminders { get; set; }
        public List<string> WelcomeMsg { get; set; }
        public bool SendWelcomeMsg { get; set; }
        public List<string> GoodbyeMsg { get; set; }
        public bool SendGoodbyeMsg { get; set; }
        public bool AdminImmedNotify { get; set; }
        public bool AdminNotifyMchanges { get; set; }
        public bool RespondToPostRequests { get; set; }
        public bool Emergency { get; set; }
        public NewMemberOptions NewMemberOptions { get; set; }
        public bool Administrivia { get; set; }
        public ushort MaxMessageSize { get; set; }
        public ushort AdminMemberChunksize { get; set; }
        public bool IncludeRfc2369Headers { get; set; }
        public bool IncludeListPostHeader { get; set; }
        public bool IncludeSenderHeader { get; set; }
        public ushort MaxDaysToHold { get; set; }

        public GeneralSection(MailmanList list) : base(list) { }
    }
}
