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
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace MailmanSharp
{
    [Flags]
    public enum SubscribeOptions { None = 0, SendWelcomeMessage = 1, NotifyOwner = 2 }
    [Flags]
    public enum UnsubscribeOptions { None = 0, SendAcknowledgement = 1, NotifyOwner = 2 }
    [Flags]
    public enum ChangeNotificationOptions { None = 0, SendToOldAddress = 1, SendToNewAddress = 2 }
    

    [Path("members")]
    [Order(4)]
    public class MembershipSection: SectionBase
    {
        /// <summary>
        /// List of all subscribed addresses, separated by newline.
        /// </summary>
        public string Emails { get { return _emailList.Cat(); } }
        /// <summary>
        /// List of all subscribed addresses.
        /// </summary>
        public IEnumerable<string> EmailList { get { return _emailList; } }

        protected static string _addPage = "members/add";
        protected static string _removePage = "members/remove";
        protected static string _changePage = "members/change";
        protected List<string> _emailList = new List<string>();

        private bool _emailListPopulated = false;
        
        internal MembershipSection(MailmanList list) : base(list) { }

        public override Task ReadAsync()
        {
            return PopulateEmailListAsync();
        }

        public override Task WriteAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        internal override JProperty GetCurrentConfigJProperty()
        {
            return null;
        }

        private async Task PopulateEmailListAsync()
        {
            _emailList.Clear();
            var resp = await this.GetClient().ExecuteRosterRequestAsync().ConfigureAwait(false);
            var doc = resp.Content.GetHtmlDocument();
            
            var addrs = doc.DocumentNode.SafeSelectNodes("//li");
            foreach (var addr in addrs)
            {
                _emailList.Add(addr.InnerText
                    .Trim()
                    .Trim('(', ')')
                    .Replace(" at ", "@"));
            }
            _emailListPopulated = true;
        }
        
        /// <summary>
        /// Set the moderation bit for all members to on.
        /// </summary>
        /// <returns></returns>
        public Task ModerateAllAsync()
        {
            return SetModerateAllAsync(true);
        }

        /// <summary>
        /// Set the moderation bit for all members to off.
        /// </summary>
        /// <returns></returns>
        public Task UnmoderateAllAsync()
        {
            return SetModerateAllAsync(false);
        }

        /// <summary>
        /// Set the moderation bit for all members to the specified value.
        /// </summary>
        /// <param name="moderate"></param>
        /// <returns></returns>
        public Task SetModerateAllAsync(bool moderate)
        {
            var req = new RestRequest(Method.POST);
            req.AddParameter("allmodbit_val", Convert.ToInt32(moderate));
            req.AddParameter("allmodbit_btn", 1);
            return this.GetClient().ExecuteAdminRequestAsync(_paths.Single(), req);
        }

        /// <summary>
        /// Unsubscribe a newline-separated list of addresses.
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<UnsubscribeResult> UnsubscribeAsync(string addresses, UnsubscribeOptions options = UnsubscribeOptions.None)
        {
            var result = new UnsubscribeResult();

            if (!String.IsNullOrWhiteSpace(addresses))
            {
                var req = new RestRequest(Method.POST);
                req.AddParameter("unsubscribees", addresses);
                req.AddParameter("send_unsub_ack_to_this_batch", options.HasFlag(UnsubscribeOptions.SendAcknowledgement).ToInt());
                req.AddParameter("send_unsub_notifications_to_list_owner", options.HasFlag(UnsubscribeOptions.NotifyOwner).ToInt());

                var resp = await this.GetClient().ExecuteAdminRequestAsync(_removePage, req).ConfigureAwait(false);
                var doc = resp.Content.GetHtmlDocument();
                
                string xpath = "//h5[contains(translate(text(), 'SU', 'su'), 'successfully unsubscribed')]/following-sibling::ul[1]/li";
                foreach (var node in doc.DocumentNode.SafeSelectNodes(xpath))
                    result.Unsubscribed.Add(node.InnerText.Trim());

                xpath = "//h3[descendant::*[contains(translate(text(), 'CU', 'cu'), 'cannot unsubscribe')]]/following-sibling::ul[1]/li";
                foreach (var node in doc.DocumentNode.SafeSelectNodes(xpath))
                    result.NonMembers.Add(node.InnerText.Trim());

                if (_emailListPopulated)
                    await PopulateEmailListAsync().ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Unsubscribe a list of members.
        /// </summary>
        /// <param name="members"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Task<UnsubscribeResult> UnsubscribeAsync(IEnumerable<string> members, UnsubscribeOptions options = UnsubscribeOptions.None)
        {
            return UnsubscribeAsync(members.Cat(), options);
        }

        /// <summary>
        /// Unsubscribe all members.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<UnsubscribeResult> UnsubscribeAllAsync(UnsubscribeOptions options = UnsubscribeOptions.None)
        {
            if (!_emailListPopulated)
                await PopulateEmailListAsync().ConfigureAwait(false);

            return await UnsubscribeAsync(EmailList, options).ConfigureAwait(false);
        }

        private enum SubscribeAction { Subscribe, Invite }
        private async Task<SubscribeResult> SubscribeOrInviteAsync(string members, string message, SubscribeAction action, SubscribeOptions options = SubscribeOptions.None)
        {
            var result = new SubscribeResult();

            if (!String.IsNullOrWhiteSpace(members))
            {
                var req = new RestRequest(Method.POST);
                req.AddParameter("subscribees", members);
                req.AddParameter("subscribe_or_invite", action == SubscribeAction.Subscribe ? 0 : 1);
                req.AddParameter("send_welcome_msg_to_this_batch", options.HasFlag(SubscribeOptions.SendWelcomeMessage).ToInt());
                req.AddParameter("send_notifications_to_list_owner", options.HasFlag(SubscribeOptions.NotifyOwner).ToInt()); 
                
                if (!String.IsNullOrWhiteSpace(message))
                {
                    // want to end with two CR, to separate from the message body
                    message = message.TrimEnd() + "\n\n";
                    req.AddParameter("invitation", message);
                }

                var resp = await this.GetClient().ExecuteAdminRequestAsync(_addPage, req).ConfigureAwait(false);
                var doc = resp.Content.GetHtmlDocument();
                
                string verb = action == SubscribeAction.Invite ? "invited" : "subscribed";
                string xpath = String.Format("//h5[contains(translate(text(), 'SI', 'si'), 'successfully {0}')]/following-sibling::ul[1]/li", verb);
                foreach (var node in doc.DocumentNode.SafeSelectNodes(xpath))
                    result.Subscribed.Add(node.InnerText.Trim());

                verb = action == SubscribeAction.Invite ? "inviting" : "subscribing";
                xpath = String.Format("//h5[contains(translate(text(), 'ESI', 'esi'),'error {0}')]/following-sibling::ul[1]/li", verb);
                foreach (var node in doc.DocumentNode.SafeSelectNodes(xpath))
                {
                    var match = Regex.Match(node.InnerText, "(.*) -- (.*)");
                    var email = match.Groups[1].Value;
                    var reason = match.Groups[2].Value;
                    if (Regex.IsMatch(reason, "Already", RegexOptions.IgnoreCase))
                        result.AlreadyMembers.Add(email);
                    else if (Regex.IsMatch(reason, "Bad/Invalid", RegexOptions.IgnoreCase))
                        result.BadEmails.Add(email);
                }

                if (action == SubscribeAction.Subscribe && _emailListPopulated)
                    await PopulateEmailListAsync().ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Subscribe a newline-separated list of addresses.
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Task<SubscribeResult> SubscribeAsync(string addresses, SubscribeOptions options = SubscribeOptions.None)
        {
            return SubscribeOrInviteAsync(addresses, null, SubscribeAction.Subscribe, options);
        }
        /// <summary>
        /// Subscribe a newline-separated list of addresses, with additional text for the notification. 
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="message"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Task<SubscribeResult> SubscribeAsync(string addresses, string message, SubscribeOptions options = SubscribeOptions.None)
        {
            return SubscribeOrInviteAsync(addresses, message, SubscribeAction.Subscribe, options);
        }
        /// <summary>
        /// Subscribe a list of addresses.
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Task<SubscribeResult> SubscribeAsync(IEnumerable<string> addresses, SubscribeOptions options = SubscribeOptions.None)
        {
            return SubscribeAsync(addresses.Cat(), options);
        }
        /// <summary>
        /// Subscribe a list of addresses, with additional text for the notification.
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="message"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Task<SubscribeResult> SubscribeAsync(IEnumerable<string> addresses, string message, SubscribeOptions options = SubscribeOptions.None)
        {
            return SubscribeAsync(addresses.Cat(), message, options);
        }
        /// <summary>
        /// Invite a newline-separated list of addresses.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public Task<SubscribeResult> InviteAsync(string addresses)
        {
            return SubscribeOrInviteAsync(addresses, null, SubscribeAction.Invite);
        }
        /// <summary>
        /// Invite a newline-separated list of addresses, with additional text for the invitation.
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<SubscribeResult> InviteAsync(string addresses, string message)
        {
            return SubscribeOrInviteAsync(addresses, message, SubscribeAction.Invite);
        }
        /// <summary>
        /// Invite a list of addresses.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public Task<SubscribeResult> InviteAsync(IEnumerable<string> addresses)
        {
            return InviteAsync(addresses.Cat());
        }
        /// <summary>
        /// Invite a list of addresses, with additional text for the invitation.
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<SubscribeResult> InviteAsync(IEnumerable<string> addresses, string message)
        {
            return InviteAsync(addresses.Cat(), message);
        }

        /// <summary>
        /// Search for members.
        /// </summary>
        /// <param name="regexp"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Member>> SearchMembersAsync(string regexp)
        {
            var req = new RestRequest(Method.GET);
            req.AddParameter("findmember", regexp);
            var resp = await this.GetClient().ExecuteAdminRequestAsync(_paths.Single(), req).ConfigureAwait(false);

            // Do we have multiple letters to look at?
            // General approach from http://www.msapiro.net/mailman-subscribers.py
            var doc = resp.Content.GetHtmlDocument();
            var letters = GetHrefValuesForParam(doc, "letter");

            if (letters.Any())
            {
                var bag = new ConcurrentBag<IEnumerable<Member>>();
                var tasks = letters.Select(async letter =>
                {
                    bag.Add(await GetMembersForLetterAsync(regexp, letter).ConfigureAwait(false));                    
                });
                await Task.WhenAll(tasks);
                return bag.SelectMany(b => b).OrderBy(m => m.Email);
            }
            else
                return ExtractMembersFromPage(doc);
        }
        /// <summary>
        /// Get a list of all members.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<Member>> GetAllMembersAsync()
        {
            return SearchMembersAsync(null);
        }


        private async Task<IEnumerable<Member>> GetMembersForLetterAsync(string search, string letter)
        {
            var result = new List<Member>();
            int currentChunk = 0;
            int maxChunk = 0;
            var req = new RestRequest(Method.GET);
            req.AddParameter("findmember", search);

            while (currentChunk <= maxChunk)
            {
                req.AddOrSetParameter("letter", letter);
                req.AddOrSetParameter("chunk", currentChunk);
                var resp = await this.GetClient().ExecuteAdminRequestAsync(_paths.Single(), req).ConfigureAwait(false);
                var doc = resp.Content.GetHtmlDocument();
                
                result.AddRange(ExtractMembersFromPage(doc));

                // More chunks?
                var nextChunk = GetHrefValuesForParam(doc, "chunk").SingleOrDefault();
                if (nextChunk != null)
                    maxChunk = Math.Max(maxChunk, Convert.ToInt32(nextChunk));
                currentChunk++;
            }

            return result;
        }

        private static IEnumerable<string> GetHrefValuesForParam(HtmlDocument doc, string param)
        {
            string re = String.Format("(?<={0}=).", param);
            var aNodes = doc.DocumentNode.SafeSelectNodes("//a[@href]");
            var values = aNodes.Select(a => a.Attributes["href"].Value)
                .Where(h => h.Contains(param + "="))
                .Select(h => Regex.Match(h, re).Value).Distinct();

            return values;
        }

        private static IEnumerable<Member> ExtractMembersFromPage(HtmlDocument doc)
        {
            var result = new List<Member>();

            var userNodes = doc.DocumentNode.SafeSelectNodes("//input[@name='user']");
            var emails = userNodes.Select(n => n.Attributes["value"].Value);

            foreach (var email in emails)
            {
                var xpath = String.Format("//input[contains(@name, '{0}')]", email);
                var nodes = doc.DocumentNode.SafeSelectNodes(xpath);
                if (nodes.Any())
                    result.Add(new Member(nodes));
            }

            return result;
        }

        /// <summary>
        /// Save changes to a list of members. Changes to the email address are not allowed.
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public Task SaveMembersAsync(IEnumerable<Member> members)
        {
            if (members.Any())
            {
                var req = new RestRequest(Method.POST);
                req.AddParameter("setmemberopts_btn", 1);

                foreach (var member in members)
                    req.Parameters.AddRange(member.ToParameters());

                return this.GetClient().ExecuteAdminRequestAsync(_paths.Single(), req);
            }
            else
                return Task.CompletedTask;
        }
        /// <summary>
        /// Save changes to an list of members.
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public Task SaveMembersAsync(params Member[] members)
        {
            return SaveMembersAsync(members.ToList());
        }

        /// <summary>
        /// Change a member's address while leaving other options the same. Note: This function
        /// is only available on Mailman 2.1.20 and higher.
        /// </summary>
        /// <param name="oldAddress"></param>
        /// <param name="newAddress"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task ChangeMemberAddressAsync(string oldAddress, string newAddress, ChangeNotificationOptions options = ChangeNotificationOptions.None)
        {
            var req = new RestRequest(Method.POST);
            req.AddParameter("change_from", oldAddress);
            req.AddParameter("change_to", newAddress);
            if (options.HasFlag(ChangeNotificationOptions.SendToOldAddress))
                req.AddParameter("notice_old", "yes");
            if (options.HasFlag(ChangeNotificationOptions.SendToNewAddress))
                req.AddParameter("notice_new", "yes");

            var resp = await this.GetClient().ExecuteAdminRequestAsync(_changePage, req).ConfigureAwait(false);
            var msg = resp.GetH3NonWarnings();
            if (!String.IsNullOrWhiteSpace(msg) && !msg.Contains("changed to"))
                throw new MailmanException(msg);
        }
    }
}
