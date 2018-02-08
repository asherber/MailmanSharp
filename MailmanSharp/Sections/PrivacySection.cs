/**
 * Copyright 2014-2018 Aaron Sherber
 * 
 * This file is part of MailmanSharp.
 *
 * MailmanSharp is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * MailmanSharp is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with MailmanSharp. If not, see <http://www.gnu.org/licenses/>.
 */

using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public enum SubscribePolicyOption { Confirm, RequireApproval, ConfirmAndApprove }
    public enum PrivateRosterOption { Anyone, ListMembers, ListAdminOnly }
    public enum MemberModerationActionOption { Hold, Reject, Discard }
    public enum GenericNonmemberActionOption { Accept, Hold, Reject, Discard }
    public enum DmarcModerationActionOption { Accept, MungeFrom, WrapMessage, Reject, Discard }

    [Path("privacy")]
    [Order(7)]
    public class PrivacySection: SectionBase
    {
        [Path("subscribing")]
        public bool? Advertised { get; set; }
        [Path("subscribing")]
        public SubscribePolicyOption? SubscribePolicy { get; set; }
        [Path("subscribing")]
        public List<string> SubscribeAutoApproval { get; set; } = new List<string>();
        [Path("subscribing")]
        public bool? UnubscribePolicy { get; set; }
        [Path("subscribing")]
        public List<string> BanList { get; set; } = new List<string>();
        [Path("subscribing")]
        public PrivateRosterOption? PrivateRoster { get; set; }
        [Path("subscribing")]
        public bool? ObscureAddresses { get; set; }

        [Path("sender")]
        public bool? DefaultMemberModeration { get; set; }
        [Path("sender")]
        public ushort? MemberVerbosityThreshold { get; set; }
        [Path("sender")]
        public ushort? MemberVerbosityInterval { get; set; }
        [Path("sender")]
        public MemberModerationActionOption? MemberModerationAction { get; set; }
        [Path("sender")]
        public string MemberModerationNotice { get; set; }
        [Path("sender")]
        public DmarcModerationActionOption? DmarcModerationAction { get; set; }
        [Path("sender")]
        public bool? DmarcQuarantineModerationAction { get; set; }
        [Path("sender")]
        public bool? DmarcNoneModerationAction { get; set; }
        [Path("sender")]
        public string DmarcModerationNotice { get; set; }
        [Path("sender")]
        public string DmarcWrappedMessageText { get; set; }
        [Path("sender")]
        public List<string> EquivalentDomains { get; set; } = new List<string>();
        [Path("sender")]
        public List<string> AcceptTheseNonmembers { get; set; } = new List<string>();
        [Path("sender")]
        public List<string> HoldTheseNonmembers { get; set; } = new List<string>();
        [Path("sender")]
        public List<string> RejectTheseNonmembers { get; set; } = new List<string>();
        [Path("sender")]
        public List<string> DiscardTheseNonmembers { get; set; } = new List<string>();
        [Path("sender")]
        public GenericNonmemberActionOption? GenericNonmemberAction { get; set; }
        [Path("sender")]
        public bool? ForwardAutoDiscards { get; set; }
        [Path("sender")]
        public string NonmemberRejectionNotice { get; set; }

        [Path("recipient")]
        public bool? RequireExplicitDestination { get; set; }
        [Path("recipient")]
        public List<string> AcceptableAliases { get; set; } = new List<string>();
        [Path("recipient")]
        public ushort? MaxNumRecipients { get; set; }

        [Path("spam")]
        public List<string> BounceMatchingHeaders { get; set; } = new List<string>();
        [Path("spam")]
        [Ignore]
        public List<HeaderFilter> FilterList { get; set; } = new List<HeaderFilter>();

        internal PrivacySection(MailmanList list) : base(list) { }

        private static readonly string _regexTag = "hdrfilter_rebox_";
        private static readonly string _actionTag = "hdrfilter_action_";

        protected override void DoAfterRead(Dictionary<string, HtmlDocument> docs)
        {
            var doc = docs.Single(d => d.Key == "privacy/spam").Value;
            
            int i = 0;
            while (doc.DocumentNode.SafeSelectNodes(String.Format("//input[@name='hdrfilter_delete_{0:D2}']", ++i)).Any())
            {
                string index = i.ToString("D2");

                this.FilterList.Add(new HeaderFilter()
                {
                    Regexes = doc.GetTextAreaListValue(_regexTag + index),
                    Action = doc.GetInputEnumValue<FilterAction>(_actionTag + index)
                });
            }
        }

        protected override void DoBeforeFinishWrite(RestRequest req)
        {
            for (int i = 0; i < this.FilterList.Count; ++i)
            {
                var filter = this.FilterList[i];
                string index = (i + 1).ToString("D2");
                req.AddParameter(_regexTag + index, filter.Regexes.Cat());
                req.AddParameter(_actionTag + index, (int)filter.Action);
            }
        }
    }
}
