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
        /// <summary>
        /// Advertise this list when people ask what lists are on this machine? 
        /// </summary>
        [Path("subscribing")]
        public bool? Advertised { get; set; }
        /// <summary>
        /// What steps are required for subscription?
        /// </summary>
        [Path("subscribing")]
        public SubscribePolicyOption? SubscribePolicy { get; set; }
        /// <summary>
        /// List of addresses (or regexps) whose subscriptions do not require approval.
        /// </summary>
        [Path("subscribing")]
        public List<string> SubscribeAutoApproval { get; set; } = new List<string>();
        /// <summary>
        /// Is the list moderator's approval required for unsubscription requests? (No is recommended.) 
        /// </summary>
        [Path("subscribing")]
        public bool? UnsubscribePolicy { get; set; }
        /// <summary>
        /// List of addresses which are banned from membership in this mailing list. 
        /// </summary>
        [Path("subscribing")]
        public List<string> BanList { get; set; } = new List<string>();
        /// <summary>
        /// Who can view subscription list?
        /// </summary>
        [Path("subscribing")]
        public PrivateRosterOption? PrivateRoster { get; set; }
        /// <summary>
        /// Show member addresses so they're not directly recognizable as email addresses?
        /// </summary>
        [Path("subscribing")]
        public bool? ObscureAddresses { get; set; }

        /// <summary>
        /// By default, should new list member postings be moderated?
        /// </summary>
        [Path("sender")]
        public bool? DefaultMemberModeration { get; set; }
        /// <summary>
        /// Ceiling on acceptable number of member posts, per interval, before automatic moderation. 
        /// </summary>
        [Path("sender")]
        public ushort? MemberVerbosityThreshold { get; set; }
        /// <summary>
        /// Number of seconds to remember posts to this list to determine member_verbosity_threshold 
        /// for automatic moderation of a member. 
        /// </summary>
        [Path("sender")]
        public ushort? MemberVerbosityInterval { get; set; }
        /// <summary>
        /// Action to take when a moderated member posts to the list.
        /// </summary>
        [Path("sender")]
        public MemberModerationActionOption? MemberModerationAction { get; set; }
        /// <summary>
        /// Text to include in any rejection notice to be sent to moderated members who post to this list.
        /// </summary>
        [Path("sender")]
        public string MemberModerationNotice { get; set; }
        /// <summary>
        /// Action to take when anyone posts to the list from a domain with a DMARC Reject/Quarantine Policy. 
        /// </summary>
        [Path("sender")]
        public DmarcModerationActionOption? DmarcModerationAction { get; set; }
        /// <summary>
        /// Should the above dmarc_moderation_action apply to messages From: domains with 
        /// DMARC p=quarantine as well as p=reject?
        /// </summary>
        [Path("sender")]
        public bool? DmarcQuarantineModerationAction { get; set; }
        /// <summary>
        /// Should the above dmarc_moderation_action apply to messages From: domains with 
        /// DMARC p=none as well as p=quarantine and p=reject 
        /// </summary>
        [Path("sender")]
        public bool? DmarcNoneModerationAction { get; set; }
        /// <summary>
        /// Text to include in any rejection notice to be sent to anyone who posts to 
        /// this list from a domain with a DMARC Reject/Quarantine Policy.
        /// </summary>
        [Path("sender")]
        public string DmarcModerationNotice { get; set; }
        /// <summary>
        /// If dmarc_moderation_action applies and is Wrap Message, and this text is provided, 
        /// the text will be placed in a separate text/plain MIME part preceding the original 
        /// message part in the wrapped message. 
        /// </summary>
        [Path("sender")]
        public string DmarcWrappedMessageText { get; set; }
        /// <summary>
        /// A 'two dimensional' list of email address domains which are considered equivalent 
        /// when checking if a post is from a list member.
        /// </summary>
        [Path("sender")]
        [MailmanIgnore]
        public List<List<string>> EquivalentDomains { get; set; } = new List<List<string>>();
        /// <summary>
        /// List of non-member addresses whose postings should be automatically accepted. 
        /// </summary>
        [Path("sender")]
        public List<string> AcceptTheseNonmembers { get; set; } = new List<string>();
        /// <summary>
        /// List of non-member addresses whose postings will be immediately held for moderation.
        /// </summary>
        [Path("sender")]
        public List<string> HoldTheseNonmembers { get; set; } = new List<string>();
        /// <summary>
        /// List of non-member addresses whose postings will be automatically rejected. 
        /// </summary>
        [Path("sender")]
        public List<string> RejectTheseNonmembers { get; set; } = new List<string>();
        /// <summary>
        /// List of non-member addresses whose postings will be automatically discarded.
        /// </summary>
        [Path("sender")]
        public List<string> DiscardTheseNonmembers { get; set; } = new List<string>();
        /// <summary>
        /// Action to take for postings from non-members for which no explicit action is defined.
        /// </summary>
        [Path("sender")]
        public GenericNonmemberActionOption? GenericNonmemberAction { get; set; }
        /// <summary>
        /// Should messages from non-members, which are automatically discarded, be forwarded to the list moderator? 
        /// </summary>
        [Path("sender")]
        public bool? ForwardAutoDiscards { get; set; }
        /// <summary>
        /// Text to include in any rejection notice to be sent to non-members who post to this list. This notice 
        /// can include the list's owner address by %(listowner)s and replaces the internally crafted default message.
        /// </summary>
        [Path("sender")]
        public string NonmemberRejectionNotice { get; set; }

        /// <summary>
        /// Must posts have list named in destination (to, cc) field (or be among the acceptable alias names, specified below)? 
        /// </summary>
        [Path("recipient")]
        public bool? RequireExplicitDestination { get; set; }
        /// <summary>
        /// Alias names (regexps) which qualify as explicit to or cc destination names for this list. 
        /// </summary>
        [Path("recipient")]
        public List<string> AcceptableAliases { get; set; } = new List<string>();
        /// <summary>
        /// Ceiling on acceptable number of recipients for a posting.
        /// </summary>
        [Path("recipient")]
        public ushort? MaxNumRecipients { get; set; }

        /// <summary>
        /// Hold posts with header value matching a specified regexp.
        /// </summary>
        [Path("spam")]
        public List<string> BounceMatchingHeaders { get; set; } = new List<string>();
        /// <summary>
        /// Filter rules to match against the headers of a message. 
        /// </summary>
        [Path("spam")]
        [MailmanIgnore]
        public List<HeaderFilterRule> HeaderFilterRules { get; set; } = new List<HeaderFilterRule>();

        internal PrivacySection(MailmanList list) : base(list) { }

        private static readonly string _regexTag = "hdrfilter_rebox_";
        private static readonly string _actionTag = "hdrfilter_action_";
        private static readonly string _equivalentDomainsTag = "equivalent_domains";

        protected override void DoAfterRead(Dictionary<string, HtmlDocument> docs)
        {
            HeaderFilterRules.Clear();
            var doc = docs.Single(d => d.Key == "privacy/spam").Value;
            
            int i = 0;
            while (doc.DocumentNode.SafeSelectNodes(String.Format("//input[@name='hdrfilter_delete_{0:D2}']", ++i)).Any())
            {
                string index = i.ToString("D2");

                this.HeaderFilterRules.Add(new HeaderFilterRule()
                {
                    Regexes = doc.GetTextAreaListValue(_regexTag + index),
                    Action = doc.GetInputEnumValue<HeaderFilterAction>(_actionTag + index).Value,
                });
            }

            EquivalentDomains.Clear();
            doc = docs.Single(d => d.Key == "privacy/sender").Value;
            var allGroups = doc.GetTextAreaStringValue(_equivalentDomainsTag);
            if (!String.IsNullOrWhiteSpace(allGroups))
            {
                var groups = allGroups.Split(';');
                foreach (var group in groups)
                {
                    var domains = group.Split(',').Select(d => d.Trim());
                    EquivalentDomains.Add(domains.ToList());
                }
            }
        }

        protected override void DoBeforeFinishWrite(RestRequest req)
        {
            for (int i = 0; i < this.HeaderFilterRules.Count; ++i)
            {
                var filter = this.HeaderFilterRules[i];
                string index = (i + 1).ToString("D2");
                req.AddParameter(_regexTag + index, filter.Regexes.Cat());
                req.AddParameter(_actionTag + index, (int)filter.Action);
            }

            var groups = EquivalentDomains.Select(g => String.Join(",", g));
            var all = String.Join(";", groups);
            req.AddParameter(_equivalentDomainsTag, all);
        }
    }
}
