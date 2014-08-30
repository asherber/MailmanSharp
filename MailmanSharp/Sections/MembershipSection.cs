using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;

namespace MailmanSharp.Sections
{
    [Path("members")]
    public class MembershipSection: SectionBase
    {
        [Ignore]
        public string EmailList { get; private set; }

        protected static string _addPage = "members/add";
        protected static string _removePage = "members/remove";
        protected static string _membersPage = "members";

        public MembershipSection(MailmanList list) : base(list) { }
        
        public override void Read()
        {
            GetEmailList();
        }

        private void GetEmailList()
        {
            var list = new List<string>();
            var resp = _list.Client.Clone().ExecuteRosterRequest();
            var doc = new MailmanHtmlDocument();
            doc.LoadHtml(resp.Content);

            var addrs = doc.DocumentNode.SafeSelectNodes("//li");
            foreach (var addr in addrs)
            {
                list.Add(addr.InnerText.Trim().Replace(" at ", "@"));
            }
            this.EmailList = String.Join("\n", list);
        }

        public void Unsubscribe(string members)
        {
            if (String.IsNullOrWhiteSpace(members)) 
                return;

            var req = new RestRequest();
            req.AddParameter("unsubscribees", members);
            req.AddParameter("send_unsub_ack_to_this_batch", 0);
            req.AddParameter("send_unsub_notifications_to_list_owner", 0);
            req.AddFile("unsubscribees_upload", new byte[0], "", "application/octet-stream");
            req.AddParameter("setmemberopts_btn", "Submit Your Changes");

            _list.Client.Clone().ExecuteAdminRequest(_removePage, req);
            GetEmailList();
        }

        public void Unsubscribe(IEnumerable<string> members)
        {
            Unsubscribe(String.Join("\n", members));
        }

        public void Unsubscribe(params string[] members)
        {
            Unsubscribe(String.Join("\n", members));
        }

        public void Subscribe(string members)
        {
            if (String.IsNullOrWhiteSpace(members))
                return;

            var req = new RestRequest();
            req.AddParameter("subscribees", members);
            req.AddParameter("subscribe_or_invite", 0);
            req.AddParameter("send_welcome_msg_to_this_batch", 0);
            req.AddParameter("send_notifications_to_list_owner", 0);
            req.AddFile("subscribees_upload", new byte[0], "", "application/octet-stream");
            req.AddParameter("setmemberopts_btn", "Submit Your Changes");

            _list.Client.Clone().ExecuteAdminRequest(_addPage, req);
            GetEmailList();
        }

        public void Subscribe(IEnumerable<string> members)
        {
            Subscribe(String.Join("\n", members));
        }

        public void Subscribe(params string[] members)
        {
            Subscribe(String.Join("\n", members));
        }

        public void Unmoderate(IEnumerable<string> members)
        {
            // This isn't great -- it assumes that members have the default values
            var req = new RestRequest();
            req.AddParameter("setmemberopts_btn", "Submit Your Changes");

            foreach (var member in members)
            {
                string addr = HttpUtility.HtmlEncode(member);
                req.AddParameter("user", addr);
                req.AddParameter(addr + "_nomail", "off");
                req.AddParameter(addr + "_nodupes", "on");
                req.AddParameter(addr + "_plain", "on");
                req.AddParameter(addr + "_language", "en");
            }

            _list.Client.Clone().ExecuteAdminRequest(_membersPage, req);
        }

        public void Unmoderate(string members)
        {
            Unmoderate(members.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
        }

        public void Unmoderate(params string[] members)
        {
            Unmoderate(members.ToList());
        }


        #region Code for reading real membership pages
        /*
        public ICollection<Member> Roster { get { return _roster; } }
        private List<Member> _roster = new List<Member>();
        public override void Read()
        {
            // There's a problem here with timing
            // The alternative is to loop through all the letter/chunk pages, which is expensive
            var general = new GeneralSection(_list);
            general.Read();
            var chunkSize = general.AdminMemberChunksize;
            general.AdminMemberChunksize = 5000;
            general.Write()

            var doc = GetHtmlDocuments().Single();
            var nodes = doc.DocumentNode.SafeSelectNodes("//input[@name='user']");
            var props = typeof(Member).GetProperties();

            foreach (var node in nodes)
            {
                var email = node.GetAttributeValue("value", null);
                var member = new Member(HttpUtility.HtmlDecode(email));

                foreach (var prop in props)
                {
                    string xpath = String.Format("//input[@name='{0}_{1}']", email, prop.Name.ToLower());
                    var propNode = doc.DocumentNode.SafeSelectNodes(xpath).FirstOrDefault();
                    if (propNode != null)
                    {
                        var val = propNode.GetAttributeValue("value", null);
                        if (prop.PropertyType == typeof(string))
                            prop.SetValue(member, val, null);
                        else if (prop.PropertyType == typeof(bool))
                            prop.SetValue(member, val == "on", null);
                    }
                }

                _roster.Add(member);
            }
        }

        public override void Write()
        {
            // do nothing
        }
        //*/
        #endregion

    }
}
