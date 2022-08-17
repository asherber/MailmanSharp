/**
 * Copyright 2014-2022 Aaron Sherber
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

using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public string AdminUrl { get { return InternalClient.AdminUrl; } set { SetMailmanVersion(""); InternalClient.AdminUrl = value; } }
        /// <summary>
        /// Administrator password for list.
        /// </summary>
        public string AdminPassword { get { return InternalClient.AdminPassword; } set { InternalClient.AdminPassword = value; } }
        /// <summary>
        /// Current configuration of object as JSON.
        /// </summary>
        public string CurrentConfig { get { return GetCurrentConfig(); } }
        /// <summary>
        /// Gets version of Mailman that this list is running on.
        /// </summary>
        public MailmanVersion MailmanVersion { get; private set; } = new MailmanVersion();

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

        /// <summary>
        /// Client which communicates with Mailman.
        /// </summary>
        public IMailmanClient Client => InternalClient;
        internal IMailmanClientInternal InternalClient { get; private set; }

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
        /// Read properties for all sections from Mailman.
        /// </summary>
        public async Task ReadAsync()
        {            
            await TryLoginAsync().ConfigureAwait(false);
            await this.InvokeSectionMethodAsync(s => s.ReadAsync()).ConfigureAwait(false);
        }

        /// <summary>
        /// Write properties for all sections to Mailman.
        /// </summary>
        public async Task WriteAsync()
        {
            await TryLoginAsync().ConfigureAwait(false);
            await this.InvokeSectionMethodAsync(s => s.WriteAsync()).ConfigureAwait(false);
        }

        private string GetCurrentConfig()
        {
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var fileVersion = $"{fvi.FileMajorPart}.{fvi.FileMinorPart}.{fvi.FileBuildPart}";

            var listProps = new JProperty("Meta", new JObject(
                new JProperty("AdminUrl", this.AdminUrl),
                new JProperty("ExportedDate", DateTime.Now.ToString("s")),
                new JProperty("MailmanVersion", this.MailmanVersion.ToString()),
                new JProperty("MailmanSharpVersion", fileVersion)
            ));

            var root = new JObject(listProps);

            foreach (var prop in _sectionProperties)
            {
                var jprop = ((SectionBase)prop.GetValue(this)).GetCurrentConfigJProperty();
                if (jprop != null)
                    root.Add(jprop);
            }
            return root.ToString();
        }



        /// <summary>
        /// Load configuration for this list from JSON string.
        /// </summary>
        /// <param name="json"></param>
        public void LoadConfig(string json)
        {
            var root = JObject.Parse(json);

            foreach (var prop in _sectionProperties)
            {
                var token = root.SelectToken(prop.Name);
                if (token != null)
                    ((SectionBase)prop.GetValue(this)).LoadConfig(token);
            }
        }

        private Task TryLoginAsync()
        {
            // This checks the URL and credentials before we get multi-threaded 
            return Client.ExecuteAdminRequestAsync(Method.GET, null);
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
        internal void SetMailmanVersion(string value)
        {
            if (value != MailmanVersion.ToString())
            {
                lock (_versionLocker)
                {
                    if (value != MailmanVersion.ToString())
                        MailmanVersion = new MailmanVersion(value);
                }
            }
        }

        /// <summary>
        /// Reset all properties to defaults.
        /// </summary>
        public void Reset()
        {
            this.ResetClient();
            InitSections();
            SetMailmanVersion("");
        }

        internal void ResetClient()
        {
            this.InternalClient = new MailmanClient(this);
        }
    }
}
