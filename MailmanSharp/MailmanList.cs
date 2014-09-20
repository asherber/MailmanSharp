using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using hap = HtmlAgilityPack;
using System.Windows.Forms;
using System.Xml.Schema;
using System.Xml;
using System.Xml.XPath;

/**
 * Tested with Mailman 2.1.17
 */

namespace MailmanSharp
{
    public class MailmanList
    {
        /// <summary>
        /// Url to the admin page for this list (e.g., http://foo.com/mailman/admin/mylist).
        /// </summary>
        public String AdminUrl { get { return Client.AdminUrl; } set { Client.AdminUrl = value; } }
        /// <summary>
        /// Administrator password for list.
        /// </summary>
        public string AdminPassword { get { return Client.AdminPassword; } set { Client.AdminPassword = value; } }
        /// <summary>
        /// Current configuration of object as XML.
        /// </summary>
        public string CurrentConfig { get { return GetCurrentConfig(); } }
        /// <summary>
        /// CurrentConfig with certain list-specific properties removed.
        /// </summary>
        public string SafeCurrentConfig { get { return GetSafeCurrentConfig(); } }

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
            
            InitSections();
        }

        public MailmanList(string adminUrl, string adminPassword = null): this()
        {
            this.AdminUrl = adminUrl;
            this.AdminPassword = adminPassword;
        }

        /// <summary>
        /// Read all list values from web site.
        /// </summary>
        public void Read()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                TryLogin();
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
                TryLogin();
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
                new XAttribute("dateCreated", DateTime.Now.ToString("s"))
            );

            foreach (var prop in GetSectionProps())
            {
                var xml = ((SectionBase)prop.GetValue(this, null)).GetCurrentConfig();
                if (!String.IsNullOrWhiteSpace(xml))
                    root.Add(XElement.Parse(xml));
            }
            return root.ToString();
        }

        private string GetSafeCurrentConfig()
        {
            return SanitizeConfig(this.CurrentConfig);
        }

        /// <summary>
        /// Remove list-specific properties from config XML.
        /// </summary>
        /// <param name="config">XML config to sanitize.</param>
        /// <returns></returns>
        public static string SanitizeConfig(string config)
        {
            var itemsToRemove = new List<string>()
            {
                "//General/RealName",
                "//General/Description",
                "//General/Info",
                "//General/SubjectPrefix",
                "//Privacy/AcceptableAliases",
            };
            var xml = XElement.Load(config);
            foreach (var item in itemsToRemove)
            {
                var el = xml.XPathSelectElement(item);
                if (el != null)
                    el.Remove();
            }

            return xml.ToString();
        }
        
        

        /// <summary>
        /// Overwrite current configuration with values from XML.
        /// </summary>
        /// <param name="xml">Config XML with values to load.</param>
        public void LoadConfig(string xml)
        {
            var root = XElement.Parse(xml);
            root.CheckElementName("MailmanList");

            foreach (var prop in GetSectionProps())
            {                
                var el = root.Element(prop.Name);
                if (el != null)
                    ((SectionBase)prop.GetValue(this, null)).LoadConfig(el.ToString());
            }
        }

        private void TryLogin()
        {
            // This checks the URL and credentials before we get multi-threaded 
            Client.ExecuteAdminRequest("");
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
            Parallel.ForEach(GetSectionProps(), p =>
            {
                var section = p.GetValue(this, null);
                if (section != null)
                    method.Invoke(section, null);
            });
        }

        private IEnumerable<PropertyInfo> GetSectionProps()
        {
            return this.GetType().GetProperties()
                .Where(p => p.PropertyType.IsSubclassOf(typeof(SectionBase)))
                .OrderBy(p => p.PropertyType.GetCustomAttributes(false).OfType<OrderAttribute>().First().Value);
        }
    }
}
