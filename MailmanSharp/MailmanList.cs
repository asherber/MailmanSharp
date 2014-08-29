using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.IO;
using MailmanSharp.Sections;
using System.Reflection;

namespace MailmanSharp
{
    public class MailmanList
    {
        public string ServerUrl { get { return Client.BaseUrl; } set { Client.BaseUrl = value; } }
        public string ListName { get { return Client.ListName; } set { Client.ListName = value; } }
        public string Password { get { return Client.Password; } set { Client.Password = value; } }

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
        public PrivacySection Privacy { get; private set; }
        public TopicsSection Topics { get; private set; }  //*/

        internal MailmanClient Client { get; private set; }
        
        
        public MailmanList()
        {
            this.Client = new MailmanClient();

            // Initialize sections
            foreach (var prop in GetSectionProps())
            {
                prop.SetValue(this, Activator.CreateInstance(prop.PropertyType, this), null);
            }
        }

        public void Read()
        {
            this.InvokeSectionMethod("Read");
        }

        public void Write()
        {
            this.InvokeSectionMethod("Write");
        }

        private void InvokeSectionMethod(string methodName)
        {
            var method = typeof(SectionBase).GetMethod(methodName);
            foreach (var prop in GetSectionProps())
            {
                var section = prop.GetValue(this, null);
                if (section != null)
                    method.Invoke(section, null);
            }
        }

        private IEnumerable<PropertyInfo> GetSectionProps()
        {
            return this.GetType().GetProperties().Where(p => p.PropertyType.IsSubclassOf(typeof(SectionBase)));
        }

        public void Login()
        {
            this.Client.BaseUrl = ServerUrl;
            this.Client.ListName = ListName;
            

            var resp = this.Client.ExecuteAdminRequest("", "adminpw", Password);
        }

        
    }
}
