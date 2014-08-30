using HtmlAgilityPack;
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

        public MembershipSection(MailmanList list) : base(list) { }
        
        public override void Read()
        {
            var list = new List<string>();
            var resp = _list.Client.ExecuteRosterRequest();
            var doc = new MailmanHtmlDocument();
            doc.LoadHtml(resp.Content);

            var addrs = doc.DocumentNode.SafeSelectNodes("//li");
            foreach (var addr in addrs)
            {
                list.Add(addr.InnerText.Trim().Replace(" at ", "@"));
            }
            this.EmailList = String.Join("\n", list);
        }

        

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

        
    }
}
