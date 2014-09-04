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

        public MembershipSection Membership { get; private set; }
        public PrivacySection Privacy { get; private set; }
        public GeneralSection General { get; private set; }
        public NonDigestSection NonDigest { get; private set; }
        public DigestSection Digest { get; private set; }
        public BounceProcessingSection BounceProcessing { get; private set; }
        public ArchivingSection Archiving { get; private set; }
        public MailNewsGatewaysSection MailNewsGateways { get; private set; }
        public ContentFilteringSection ContentFiltering { get; private set; }
        public PasswordsSection Passwords { get; private set; }   
        public AutoResponderSection AutoResponder { get; private set; }  

        /*
        public LanguageSection Language { get; private set; }   // Could implement; not necessary
        public TopicsSection Topics { get; private set; }  // Won't implement     //*/

        internal MailmanClient Client { get; private set; }

        public MailmanList()
        {
            this.Client = new MailmanClient();
            ThreadPool.SetMaxThreads(10, 10);
            ThreadPool.SetMinThreads(10, 10);

            InitSections();
        }

        public void Read()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                this.InvokeSectionMethod("Read");
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

        public string GetConfig()
        {
            var root = new XElement("MailmanList",
                new XElement("BaseUrl", BaseUrl),
                new XElement("ListName", ListName),
                new XElement("Password", Password)
            );

            foreach (var prop in GetSectionProps())
            {
                var xml = ((SectionBase)prop.GetValue(this, null)).GetConfig();
                root.Add(XElement.Parse(xml));
            }
            return root.ToString();
        }

        public void MergeConfig(string xml)
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
                    ((SectionBase)prop.GetValue(this, null)).MergeConfig(el.ToString());
            }
        }

        public void LoadConfig(string xml)
        {
            InitSections();
            MergeConfig(xml);
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
