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
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

/**
 * Tested with Mailman 2.1.23
 */

namespace MailmanSharp
{
    public class MailmanList: IMailmanList
    {
        /// <summary>
        /// Url to the admin page for this list (e.g., http://foo.com/mailman/admin/mylist).
        /// </summary>
        public string AdminUrl { get { return InternalClient.AdminUrl; } set { MailmanVersion = null; InternalClient.AdminUrl = value; } }
        /// <summary>
        /// Administrator password for list.
        /// </summary>
        public string AdminPassword { get { return InternalClient.AdminPassword; } set { InternalClient.AdminPassword = value; } }
        /// <summary>
        /// Current configuration of object as XML.
        /// </summary>
        public string CurrentConfig { get { return GetCurrentConfig(); } }
        /// <summary>
        /// CurrentConfig with certain list-specific properties removed.
        /// </summary>
        public string SafeCurrentConfig { get { return GetSafeCurrentConfig(); } }
        /// <summary>
        /// Gets version of Mailman that this list is running on.
        /// </summary>
        public string MailmanVersion { get { return _mailmanVersion; } internal set { SetMailmanVersion(value); } }

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

        public IMailmanClient Client => InternalClient;
        internal IMailmanClientInternal InternalClient { get; private set; }

        private string _mailmanVersion = null;

        private static IEnumerable<PropertyInfo> _sectionProperties;

        static MailmanList()
        {
            _sectionProperties = typeof(MailmanList).GetProperties()
                .Where(p => p.PropertyType.IsSubclassOf(typeof(SectionBase)))
                .OrderBy(p => p.PropertyType.GetCustomAttributes(false).OfType<OrderAttribute>().First().Value)
                .ToList();
        }

        public MailmanList()
        {
            this.Reset();
        }

        public MailmanList(string adminUrl, string adminPassword = null): this()
        {
            this.AdminUrl = adminUrl;
            this.AdminPassword = adminPassword;
        }

        /// <summary>
        /// Read all list values from web site.
        /// </summary>
        public async Task ReadAsync()
        {            
            await TryLoginAsync().ConfigureAwait(false);
            await this.InvokeSectionMethodAsync(s => s.ReadAsync()).ConfigureAwait(false);
        }

        /// <summary>
        /// Write all values to list.
        /// </summary>
        public async Task WriteAsync()
        {
            await TryLoginAsync().ConfigureAwait(false);
            await this.InvokeSectionMethodAsync(s => s.WriteAsync()).ConfigureAwait(false);
        }

        private string GetCurrentConfig()
        {
            var root = new XElement("MailmanList",
                new XAttribute("adminUrl", this.AdminUrl),
                new XAttribute("dateCreated", DateTime.Now.ToString("s")),
                new XAttribute("mailmanVersion", this.MailmanVersion),
                new XAttribute("mailmanSharpVersion", Assembly.GetExecutingAssembly().GetName().Version)
            );

            foreach (var prop in _sectionProperties)
            {
                var xml = ((SectionBase)prop.GetValue(this)).GetCurrentConfig();
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
            var xml = XElement.Parse(config);
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

            foreach (var prop in _sectionProperties)
            {                
                var el = root.Element(prop.Name);
                if (el != null)
                    ((SectionBase)prop.GetValue(this)).LoadConfig(el.ToString());
            }
        }

        private Task TryLoginAsync()
        {
            // This checks the URL and credentials before we get multi-threaded 
            return Client.ExecuteGetAdminRequestAsync("");
        }

        private string GetNodeValue(XElement root, string nodeName)
        {
            var el = root.Element(nodeName);
            return el != null ? el.Value : null;
        }

        private void InitSections()
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var args = new object[] { this };

            foreach (var prop in _sectionProperties)
            {
                var section = Activator.CreateInstance(prop.PropertyType, flags, null, args, null);
                prop.SetValue(this, section);
            }
        }

        private Task InvokeSectionMethodAsync(Func<SectionBase, Task> func)
        {
            var tasks = _sectionProperties.Select(p =>
            {
                if (p.GetValue(this) is SectionBase section)
                    return func(section);
                else
                    return Task.CompletedTask;
            });
            return Task.WhenAll(tasks);
        }

        private object _versionLocker = new object();
        private void SetMailmanVersion(string value)
        {
            lock (_versionLocker)
            {
                if (value != _mailmanVersion)
                    _mailmanVersion = value;
            }
        }

        public void Reset()
        {
            this.ResetClient();
            InitSections();
            this.MailmanVersion = null;
        }

        internal void ResetClient()
        {
            this.InternalClient = new MailmanClient(this);
        }
    }
}
