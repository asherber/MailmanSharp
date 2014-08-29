using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    public enum SubscribePolicyOption { Confirm, RequireApproval, ConfirmAndApprove }
    public enum PrivateRosterOption { Anyone, ListMembers, ListAdminOnly }
    public enum MemberModerationActionOption { Hold, Reject, Discard }
    public enum GenericNonmemberActionOption { Accept, Hold, Reject, Discard }

    [Path("privacy")]
    public class PrivacySection: SectionBase
    {
        [Path("subscribing")]
        public bool Advertised { get; set; }
        [Path("subscribing")]
        public SubscribePolicyOption SubscribePolicy { get; set; }
        [Path("subscribing")]
        public bool UnubscribePolicy { get; set; }
        [Path("subscribing")]
        public List<string> BanList { get; set; }
        [Path("subscribing")]
        public PrivateRosterOption PrivateRoster { get; set; }
        [Path("subscribing")]
        public bool ObscureAddresses { get; set; }

        [Path("sender")]
        public bool DefaultMemberModeration { get; set; }
        [Path("sender")]
        public MemberModerationActionOption MemberModerationAction { get; set; }
        [Path("sender")]
        public List<string> MemberModerationNotice { get; set; }
        [Path("sender")]
        public List<string> AcceptTheseNonmembers { get; set; }
        [Path("sender")]
        public List<string> HoldTheseNonmembers { get; set; }
        [Path("sender")]
        public List<string> RejectTheseNonmembers { get; set; }
        [Path("sender")]
        public List<string> DiscardTheseNonmembers { get; set; }
        [Path("sender")]
        public GenericNonmemberActionOption GenericNonmemberAction { get; set; }
        [Path("sender")]
        public bool ForwardAutoDiscards { get; set; }
        [Path("sender")]
        public List<string> NonmemberRejectionNotice { get; set; }

        [Path("recipient")]
        public bool RequireExplicitDestination { get; set; }
        [Path("recipient")]
        public List<string> AcceptableAliases { get; set; }
        [Path("recipient")]
        public ushort MaxNumRecipients { get; set; }

        // Spam page -- will not implement

        public PrivacySection(MailmanList list) : base(list) { }
    }
}
