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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public enum FromIsListOption { No, MungeFrom, WrapMessage }
    public enum ReplyGoesToListOption { Poster, ThisList, ExplicitAddress }
    [Flags]
    public enum NewMemberOptions { None = 0, Hide = 1, Ack = 2, NotMeToo = 4, NoDupes = 8 }

    [Path("general")]
    [Order(1)]
    public class GeneralSection: SectionBase
    {
        /// <summary>
        /// The public name of this list (make case-changes only). 
        /// </summary>
        public string RealName { get { return _realName; } set { SetRealName(value); } }
        /// <summary>
        /// The list administrator email addresses.
        /// </summary>
        public List<string> Owner { get; set; } = new List<string>();
        /// <summary>
        /// The list moderator email addresses.
        /// </summary>
        public List<string> Moderator { get; set; } = new List<string>();
        /// <summary>
        /// A terse phrase identifying this list.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// An introductory description – a few paragraphs – about the list. 
        /// It will be included, as html, at the top of the listinfo page.
        /// </summary>
        public string Info { get; set; }
        /// <summary>
        /// Prefix for subject line of list postings.
        /// </summary>
        public string SubjectPrefix { get; set; }
        /// <summary>
        /// Replace the From: header address with the list's posting address to mitigate 
        /// issues stemming from the original From: domain's DMARC or similar policies. 
        /// </summary>
        public FromIsListOption? FromIsList { get; set; }
        /// <summary>
        /// Hide the sender of a message, replacing it with the list address (Removes From, Sender and Reply-To fields) 
        /// </summary>
        public bool? AnonymousList { get; set; }
        /// <summary>
        /// Should any existing Reply-To: header found in the original message be stripped? 
        /// If so, this will be done regardless of whether an explict Reply-To: header is added by Mailman or not. 
        /// </summary>
        public bool? FirstStripReplyTo { get; set; }
        /// <summary>
        /// Where are replies to list messages directed? Poster is strongly recommended for most mailing lists. 
        /// </summary>
        public ReplyGoesToListOption? ReplyGoesToList { get; set; }
        /// <summary>
        /// Explicit Reply-To: header. 
        /// </summary>
        public string ReplyToAddress { get; set; }
        /// <summary>
        /// Send password reminders to, eg, "-owner" address instead of directly to user.
        /// </summary>
        public bool? UmbrellaList { get; set; }
        /// <summary>
        /// Suffix for use when this list is an umbrella for other lists, 
        /// according to setting of previous "umbrella_list" setting. 
        /// </summary>
        public string UmbrellaMemberSuffix { get; set; }
        /// <summary>
        /// Send monthly password reminders?
        /// </summary>
        public bool? SendReminders { get; set; }
        /// <summary>
        /// List-specific text prepended to new-subscriber welcome message.
        /// </summary>
        public string WelcomeMsg { get; set; }
        /// <summary>
        /// Send welcome message to newly subscribed members? 
        /// </summary>
        public bool? SendWelcomeMsg { get; set; }
        /// <summary>
        /// Text sent to people leaving the list. If empty, no special text will be added to the unsubscribe message.
        /// </summary>
        public string GoodbyeMsg { get; set; }
        /// <summary>
        /// Send goodbye message to members when they are unsubscribed?
        /// </summary>
        public bool? SendGoodbyeMsg { get; set; }
        /// <summary>
        /// Should the list moderators get immediate notice of new requests, as well as daily notices about collected ones?
        /// </summary>
        public bool? AdminImmedNotify { get; set; }
        /// <summary>
        /// Should administrator get notices of subscribes and unsubscribes? 
        /// </summary>
        public bool? AdminNotifyMchanges { get; set; }
        /// <summary>
        /// Send mail to poster when their posting is held for approval? 
        /// </summary>
        public bool? RespondToPostRequests { get; set; }
        /// <summary>
        /// Emergency moderation of all list traffic. 
        /// </summary>
        public bool? Emergency { get; set; }
        /// <summary>
        /// Default options for new members joining this list.
        /// </summary>
        public NewMemberOptions? NewMemberOptions { get; set; }
        /// <summary>
        /// Check postings and intercept ones that seem to be administrative requests? 
        /// </summary>
        public bool? Administrivia { get; set; }
        /// <summary>
        /// Maximum length in kilobytes (KB) of a message body. Use 0 for no limit.
        /// </summary>
        public ushort? MaxMessageSize { get; set; }
        /// <summary>
        /// Maximum number of members to show on one page of the Membership List.
        /// </summary>
        public ushort? AdminMemberChunksize { get; set; }
        /// <summary>
        /// Should messages from this mailing list include the RFC 2369 (i.e. List-*) headers? Yes is highly recommended. 
        /// </summary>
        public bool? IncludeRfc2369Headers { get; set; }
        /// <summary>
        /// Should postings include the List-Post: header?
        /// </summary>
        public bool? IncludeListPostHeader { get; set; }
        /// <summary>
        /// Should the Sender header be rewritten for this mailing list to avoid stray bounces? Yes is recommended. 
        /// </summary>
        public bool? IncludeSenderHeader { get; set; }
        /// <summary>
        /// Discard held messages older than this number of days. Use 0 for no automatic discarding.
        /// </summary>
        public ushort? MaxDaysToHold { get; set; }

        private string _realName = "";

        internal GeneralSection(MailmanList list) : base(list) { }

        private void SetRealName(string value)
        {
            // Only allow case changes
            if (value != _realName)
            {
                if (!String.IsNullOrWhiteSpace(_realName) && String.Compare(value, _realName, true) != 0)
                    throw new ArgumentOutOfRangeException("RealName", "RealName can only differ by case.");
                else
                    _realName = value;
            }
        }
    }
}
