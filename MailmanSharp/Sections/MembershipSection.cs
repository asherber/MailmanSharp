using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MailmanSharp
{
    [Path("members")]
    [Order(4)]
    public class MembershipSection: SectionBase
    {
        public string Emails { get { return String.Join("\n", _emailList); } }
        public IEnumerable<string> EmailList { get { return _emailList; } }

        protected static string _addPage = "members/add";
        protected static string _removePage = "members/remove";
        protected List<string> _emailList = new List<string>();
        
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
            var resp = this.Client.ExecuteRosterRequest();
            var doc = new MailmanHtmlDocument();
            doc.LoadHtml(resp.Content);

            var addrs = doc.DocumentNode.SafeSelectNodes("//li");
            foreach (var addr in addrs)
            {
                _emailList.Add(addr.InnerText.Trim().Replace(" at ", "@"));
            }
        }

        public void ModerateAll(bool moderate)
        {
            var req = new RestRequest();
            req.AddParameter("allmodbit_val", moderate ? 1 : 0);
            req.AddParameter("allmodbit_btn", 1);
            this.Client.ExecuteAdminRequest(_paths.Single(), req);
        }

        public UnsubscribeResult Unsubscribe(string members)
        {
            var result = new UnsubscribeResult();

            if (!String.IsNullOrWhiteSpace(members))
            {
                var req = new RestRequest();
                req.AddParameter("unsubscribees", members);
                req.AddParameter("send_unsub_ack_to_this_batch", 0);
                req.AddParameter("send_unsub_notifications_to_list_owner", 0);

                var resp = this.Client.ExecuteAdminRequest(_removePage, req);
                var doc = new MailmanHtmlDocument();
                doc.LoadHtml(resp.Content);

                string xpath = "//h5[contains(translate(text(), 'SU', 'su'), 'successfully unsubscribed')]/following-sibling::ul[1]/li";
                foreach (var node in doc.DocumentNode.SafeSelectNodes(xpath))
                    result.Unsubscribed.Add(node.InnerText.Trim());

                xpath = "//h3[descendant::*[contains(translate(text(), 'CU', 'cu'), 'cannot unsubscribe')]]/following-sibling::ul[1]/li";
                foreach (var node in doc.DocumentNode.SafeSelectNodes(xpath))
                    result.NonMembers.Add(node.InnerText.Trim());

                PopulateEmailList();
            }

            return result;
        }

        public UnsubscribeResult Unsubscribe(IEnumerable<string> members)
        {
            return Unsubscribe(String.Join("\n", members));
        }

        public UnsubscribeResult Unsubscribe(params string[] members)
        {
            return Unsubscribe(String.Join("\n", members));
        }

        public SubscribeResult Subscribe(string members)
        {
            var result = new SubscribeResult();

            if (!String.IsNullOrWhiteSpace(members))
            {
                var req = new RestRequest();
                req.AddParameter("subscribees", members);
                req.AddParameter("subscribe_or_invite", 0);
                req.AddParameter("send_welcome_msg_to_this_batch", 0);
                req.AddParameter("send_notifications_to_list_owner", 0);

                var resp = this.Client.ExecuteAdminRequest(_addPage, req);
                var doc = new MailmanHtmlDocument();
                doc.LoadHtml(resp.Content);

                string xpath = "//h5[contains(translate(text(), 'S', 's'), 'successfully subscribed')]/following-sibling::ul[1]/li";
                foreach (var node in doc.DocumentNode.SafeSelectNodes(xpath))
                    result.Subscribed.Add(node.InnerText.Trim());

                xpath = "//h5[contains(translate(text(), 'ES', 'es'),'error subscribing')]/following-sibling::ul[1]/li";
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

                PopulateEmailList();
            }

            return result;
        }

        public SubscribeResult Subscribe(IEnumerable<string> members)
        {
            return Subscribe(String.Join("\n", members));
        }

        public SubscribeResult Subscribe(params string[] members)
        {
            return Subscribe(String.Join("\n", members));
        }

        public IList<Member> GetMembers(string search)
        {
            var result = new List<Member>();
            var req = new RestRequest();
            req.AddParameter("findmember", search);
            var resp = this.Client.ExecuteAdminRequest(_paths.Single(), req);

            var doc = new HtmlDocument();
            doc.LoadHtml(resp.Content);

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

            this.Client.ExecuteAdminRequest(_paths.Single(), req);
        }

        public void SaveMembers(params Member[] members)
        {
            SaveMembers(members.ToList());
        }

        public IEnumerable<Member> GetAllMembers()
        {
            var result = new List<Member>();

            // Increase chunk size
            var general = new GeneralSection(_list);
            general.Read();
            var chunkSize = general.AdminMemberChunksize;
            general.AdminMemberChunksize = 10000;
            general.Write();

            try
            {
                // Get all user nodes
                var doc = GetHtmlDocuments().Single();
                var nodes = doc.DocumentNode.SafeSelectNodes("//input[@name='user']");

                foreach (var node in nodes)
                {
                    var email = node.GetAttributeValue("value", null);
                    var xpath = String.Format("//input[contains(@name, '{0}')]", email);
                    var memberNodes = doc.DocumentNode.SafeSelectNodes(xpath);

                    result.Add(new Member(memberNodes));
                }
            }
            finally
            {
                // Reset chunk size
                general.AdminMemberChunksize = chunkSize;
                general.Write();
            }

            return result;
        }
    }
}
