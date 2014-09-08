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

/**
 * Tested with Mailman 2.1.17
 * 
 * TODO: Require login to check RestResponse
 * 
 */

namespace MailmanSharp
{
    public class MailmanList
    {
        public String AdminUrl { get { return Client.AdminUrl; } set { Client.AdminUrl = value; } }
        public string AdminPassword { get { return Client.AdminPassword; } set { Client.AdminPassword = value; } }
        public string CurrentConfig { get { return GetCurrentConfig(); } }

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
        public TopicsSection Topics { get; private set; }     

        //public LanguageSection Language { get; private set; }   // Won't implement

        internal MailmanClient Client { get; private set; }

        public MailmanList()
        {
            this.Client = new MailmanClient();
            ThreadPool.SetMaxThreads(10, 10);
            ThreadPool.SetMinThreads(10, 10);            

            InitSections();
        }

        public MailmanList(string adminUrl, string adminPassword = null): this()
        {
            AdminUrl = adminUrl;
            AdminPassword = adminPassword;
        }

        /// <summary>
        /// Read all list values from web site.
        /// </summary>
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

        /// <summary>
        /// Write all values to list.
        /// </summary>
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

        protected string GetCurrentConfig()
        {
            var root = new XElement("MailmanList",
                new XAttribute("adminUrl", this.AdminUrl),
                new XAttribute("dateCreated", DateTime.Now)
            );

            foreach (var prop in GetSectionProps())
            {
                var xml = ((SectionBase)prop.GetValue(this, null)).GetCurrentConfig();
                if (!String.IsNullOrWhiteSpace(xml))
                    root.Add(XElement.Parse(xml));
            }
            return root.ToString();
        }

        /// <summary>
        /// Overwrite current configuration with values from XML.
        /// </summary>
        /// <param name="xml">Config XML with values to load.</param>
        public void LoadConfig(string xml)
        {
            var root = XElement.Parse(xml);
            foreach (var prop in GetSectionProps())
            {                
                var el = root.Element(prop.Name);
                if (el != null)
                    ((SectionBase)prop.GetValue(this, null)).LoadConfig(el.ToString());
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
            return this.GetType().GetProperties()
                .Where(p => p.PropertyType.IsSubclassOf(typeof(SectionBase)))
                .OrderBy(p => p.PropertyType.GetCustomAttributes(false).OfType<OrderAttribute>().First().Value);
        }
    }
}
