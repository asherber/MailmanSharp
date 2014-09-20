using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using forms = System.Windows.Forms;

namespace MailmanSharp
{
    [Flags]
    public enum SubscribeOptions { None = 0, SendWelcomeMessage = 1, NotifyOwner = 2 }
    [Flags]
    public enum UnsubscribeOptions { None = 0, SendAcknowledgement = 1, NotifyOwner = 2 }
    

    [Path("members")]
    [Order(4)]
    public class MembershipSection: SectionBase
    {
        public string Emails { get { return String.Join("\n", _emailList); } }
        public IEnumerable<string> EmailList { get { return _emailList; } }

        protected static string _addPage = "members/add";
        protected static string _removePage = "members/remove";
        protected List<string> _emailList = new List<string>();

        private bool _emailListPopulated = false;
        
        public MembershipSection(MailmanList list) : base(list) { }

        public override void Read()
        {
            PopulateEmailList();
        }

        public override void Write()
        {
            // do nothing
        }

        internal override string GetCurrentConfig()
        {
            return null;
        }

        private void PopulateEmailList()
        {
            _emailList.Clear();
            var resp = this.Client.ExecuteRosterRequest();
            var doc = new MailmanHtmlDocument();
            doc.LoadHtml(resp.Content);

            var addrs = doc.DocumentNode.SafeSelectNodes("//li");
            foreach (var addr in addrs)
            {
                _emailList.Add(addr.InnerText.Trim().Replace(" at ", "@"));
            }
            _emailListPopulated = true;
        }

        public void ModerateAll(bool moderate)
        {
            var req = new RestRequest();
            req.AddParameter("allmodbit_val", moderate ? 1 : 0);
            req.AddParameter("allmodbit_btn", 1);
            this.Client.PostAdminRequest(_paths.Single(), req);
        }

        public UnsubscribeResult Unsubscribe(string members, UnsubscribeOptions options = UnsubscribeOptions.None)
        {
            var result = new UnsubscribeResult();

            if (!String.IsNullOrWhiteSpace(members))
            {
                var req = new RestRequest();
                req.AddParameter("unsubscribees", members);
                req.AddParameter("send_unsub_ack_to_this_batch", options.HasFlag(UnsubscribeOptions.SendAcknowledgement).ToInt());
                req.AddParameter("send_unsub_notifications_to_list_owner", options.HasFlag(UnsubscribeOptions.None).ToInt());

                var resp = this.Client.PostAdminRequest(_removePage, req);
                var doc = new MailmanHtmlDocument();
                doc.LoadHtml(resp.Content);

                string xpath = "//h5[contains(translate(text(), 'SU', 'su'), 'successfully unsubscribed')]/following-sibling::ul[1]/li";
                foreach (var node in doc.DocumentNode.SafeSelectNodes(xpath))
                    result.Unsubscribed.Add(node.InnerText.Trim());

                xpath = "//h3[descendant::*[contains(translate(text(), 'CU', 'cu'), 'cannot unsubscribe')]]/following-sibling::ul[1]/li";
                foreach (var node in doc.DocumentNode.SafeSelectNodes(xpath))
                    result.NonMembers.Add(node.InnerText.Trim());

                if (_emailListPopulated)
                    PopulateEmailList();
            }

            return result;
        }

        public UnsubscribeResult Unsubscribe(IEnumerable<string> members, UnsubscribeOptions options = UnsubscribeOptions.None)
        {
            return Unsubscribe(String.Join("\n", members), options);
        }

        private enum SubscribeAction { Subscribe, Invite }
        private SubscribeResult SubscribeOrInvite(string members, SubscribeAction action, SubscribeOptions options = SubscribeOptions.None)
        {
            var result = new SubscribeResult();

            if (!String.IsNullOrWhiteSpace(members))
            {
                var req = new RestRequest();
                req.AddParameter("subscribees", members);
                req.AddParameter("subscribe_or_invite", action == SubscribeAction.Subscribe ? 0 : 1);
                req.AddParameter("send_welcome_msg_to_this_batch", options.HasFlag(SubscribeOptions.SendWelcomeMessage).ToInt());
                req.AddParameter("send_notifications_to_list_owner", options.HasFlag(SubscribeOptions.NotifyOwner).ToInt());

                var resp = this.Client.PostAdminRequest(_addPage, req);
                var doc = new MailmanHtmlDocument();
                doc.LoadHtml(resp.Content);

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
                    PopulateEmailList();
            }

            return result;
        }

        public SubscribeResult Subscribe(string members, SubscribeOptions options = SubscribeOptions.None)
        {
            return SubscribeOrInvite(members, SubscribeAction.Subscribe, options);
        }

        public SubscribeResult Subscribe(IEnumerable<string> members, SubscribeOptions options = SubscribeOptions.None)
        {
            return Subscribe(String.Join("\n", members), options);
        }

        public SubscribeResult Invite(string members)
        {
            return SubscribeOrInvite(members, SubscribeAction.Invite);
        }

        public SubscribeResult Invite(IEnumerable<string> members)
        {
            return Invite(String.Join("\n", members));
        }

        public IEnumerable<Member> GetMembers()
        {
            return GetMembers("");
        }

        public IEnumerable<Member> GetMembers(string search)
        {
            var req = new RestRequest();
            req.AddParameter("findmember", search);
            var resp = this.Client.ExecuteAdminRequest(_paths.Single(), req);

            // Do we have multiple letters to look at?
            // General approach from http://www.msapiro.net/mailman-subscribers.py
            var doc = new HtmlDocument();
            doc.LoadHtml(resp.Content);
            var letters = GetHrefValuesForParam(doc, "letter");

            if (letters.Any())
            {
                forms.Cursor.Current = forms.Cursors.WaitCursor;
                try
                {
                    var list = new List<Member>();
                    Parallel.ForEach(letters, letter =>
                    {
                        var members = GetMembersForLetter(search, letter);
                        lock (list) { list.AddRange(members); }
                    });

                    return list.OrderBy(m => m.Email);
                }
                finally
                {
                    forms.Cursor.Current = forms.Cursors.Default;
                }
                
            }
            else
                return ExtractMembersFromPage(doc);            
        }

        private IEnumerable<Member> GetMembersForLetter(string search, string letter)
        {
            var result = new List<Member>();
            int currentChunk = 0;
            int maxChunk = 0;
            var req = new RestRequest();
            req.AddParameter("findmember", search);

            while (currentChunk <= maxChunk)
            {
                req.AddOrSetParameter("letter", letter);
                req.AddOrSetParameter("chunk", currentChunk);
                var resp = this.Client.ExecuteAdminRequest(_paths.Single(), req);
                var doc = new HtmlDocument();
                doc.LoadHtml(resp.Content);

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
            var values = aNodes.Select(a => a.GetAttributeValue("href", ""))
                .Where(h => h.Contains(param + "="))
                .Select(h => Regex.Match(h, re).Value).Distinct();

            return values;
        }

        private static IEnumerable<Member> ExtractMembersFromPage(HtmlDocument doc)
        {
            var result = new List<Member>();

            var userNodes = doc.DocumentNode.SafeSelectNodes("//input[@name='user']");
            var emails = userNodes.Select(n => n.GetAttributeValue("value", null));

            foreach (var email in emails)
            {
                var xpath = String.Format("//input[contains(@name, '{0}')]", email);
                var nodes = doc.DocumentNode.SafeSelectNodes(xpath);
                if (nodes.Any())
                    result.Add(new Member(nodes));
            }

            return result;
        }

        public void SaveMembers(IEnumerable<Member> members)
        {
            var req = new RestRequest();
            req.AddParameter("setmemberopts_btn", 1);

            foreach (var member in members)
                req.Parameters.AddRange(member.ToParameters());

            this.Client.PostAdminRequest(_paths.Single(), req);
        }

        public void SaveMembers(params Member[] members)
        {
            SaveMembers(members.ToList());
        }

        
    }
}
