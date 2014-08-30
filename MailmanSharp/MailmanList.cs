using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.IO;
using MailmanSharp.Sections;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using hap = HtmlAgilityPack;
using System.Windows.Forms;


namespace MailmanSharp
{
    public class MailmanList
    {
        public string BaseUrl { get { return Client.BaseUrl; } set { Client.BaseUrl = value; } }
        public string ListName { get { return Client.ListName; } set { Client.ListName = value; } }
        public string Password { get { return Client.Password; } set { Client.Password = value; } }

        public PrivacySection Privacy { get; private set; }
        public GeneralSection General { get; private set; }
        public NonDigestSection NonDigest { get; private set; }
        public DigestSection Digest { get; private set; }
        public BounceProcessingSection BounceProcessing { get; private set; }
        public ArchivingSection Archiving { get; private set; }
        public MailNewsGatewaysSection MailNewsGateways { get; private set; }
        public ContentFilteringSection ContentFiltering { get; private set; }
        /*
        public AutoResponderSection AutoResponder { get; private set; }
        public LanguageSection Language { get; private set; }
        public MembershipSection Membership { get; private set; }
        public PasswordsSection Passwords { get; private set; }
        public TopicsSection Topics { get; private set; }  //*/

        public ReadOnlyCollection<string> CurrentSubscribers { get { return _currentSubscribers.AsReadOnly(); } }

        internal MailmanClient Client { get; private set; }

        private List<string> _currentSubscribers;
        
        public MailmanList()
        {
            this.Client = new MailmanClient();
            _currentSubscribers = new List<string>();
            ThreadPool.SetMaxThreads(10, 10);
            ThreadPool.SetMinThreads(10, 10);

            InitSections();
        }

        public void Read()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var tasks = new Task[]
                {
                    Task.Factory.StartNew(() => this.InvokeSectionMethod("Read")),
                    Task.Factory.StartNew(() => GetCurrentSubscribers()),
                };
                Task.WaitAll(tasks);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public void Write()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                this.InvokeSectionMethod("Write");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public string Serialize()
        {
            var root = new XElement("MailmanList",
                new XElement("BaseUrl", BaseUrl),
                new XElement("ListName", ListName),
                new XElement("Password", Password)
            );

            foreach (var prop in GetSectionProps())
            {
                var xml = ((SectionBase)prop.GetValue(this, null)).Serialize();
                root.Add(XElement.Parse(xml));
            }
            return root.ToString();
        }

        public void MergeValues(string xml)
        {
            var root = XElement.Parse(xml);
            BaseUrl = GetNodeValue(root, "BaseUrl") ?? BaseUrl;
            ListName = GetNodeValue(root, "ListName") ?? ListName;
            Password = GetNodeValue(root, "Password") ?? Password;

            foreach (var prop in GetSectionProps())
            {
                var nodeName = prop.Name.Replace("Section", "");
                var el = root.Element(nodeName);
                if (el != null)
                    ((SectionBase)prop.GetValue(this, null)).MergeValues(el.ToString());
            }
        }

        public void LoadValues(string xml)
        {
            InitSections();
            MergeValues(xml);
        }

        public void GetCurrentSubscribers()
        {
            _currentSubscribers.Clear();
            var resp = Client.ExecuteRosterRequest();
            // The <li> tags on this page are unclosed
            var doc = new hap.HtmlDocument() { OptionFixNestedTags = true };
            doc.LoadHtml(resp.Content);

            var addrs = doc.DocumentNode.SafeSelectNodes("//li");
            foreach (var addr in addrs)
            {
                _currentSubscribers.Add(addr.InnerText.Trim().Replace(" at ", "@"));
            }
        }

        private string GetNodeValue(XElement root, string nodeName)
        {
            var el = root.Element(nodeName);
            return el != null ? el.Value : null;
        }

        private void InitSections()
        {
            foreach (var prop in GetSectionProps())
            {
                prop.SetValue(this, Activator.CreateInstance(prop.PropertyType, this), null);
            }
        }

        private void InvokeSectionMethod(string methodName)
        {
            var method = typeof(SectionBase).GetMethod(methodName);
            var tasks = new List<Task>();
            foreach (var prop in GetSectionProps())
            {
                var section = prop.GetValue(this, null);
                if (section != null)
                    tasks.Add(Task.Factory.StartNew(() => method.Invoke(section, null)));                
            }
            Task.WaitAll(tasks.ToArray());
        }

        private IEnumerable<PropertyInfo> GetSectionProps()
        {
            return this.GetType().GetProperties().Where(p => p.PropertyType.IsSubclassOf(typeof(SectionBase)));
        }

        

        
    }
}
