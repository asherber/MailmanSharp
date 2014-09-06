using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MailmanSharp
{
    public class Member
    {
        public string Email { get; internal set; }
        public string RealName { get; set; }
        public bool Mod { get; set; }
        public bool Hide { get; set; }
        public bool NoMail { get; set; }
        public bool Ack { get; set; }
        public bool NotMeToo { get; set; }
        public bool NoDupes { get; set; }
        public bool Digest { get; set; }
        public bool Plain { get; set; }

        protected string _encEmail;
        
        internal Member(HtmlNodeCollection nodes)
        {
            var firstNode = nodes.First();
            _encEmail = Regex.Replace(firstNode.GetAttributeValue("name", null), "_\\w*$", "");
            this.Email = HttpUtility.UrlDecode(_encEmail);

            foreach (var prop in this.GetType().GetProperties())
            {
                var name = String.Format("{0}_{1}", _encEmail, prop.Name.ToLower());
                var thisNode = nodes.SingleOrDefault(n => n.GetAttributeValue("name", null) == name);
                if (thisNode != null)
                {
                    var val = thisNode.GetAttributeValue("value", null);
                    if (prop.PropertyType == typeof(string))
                        prop.SetValue(this, val, null);
                    else
                        prop.SetValue(this, val == "on", null);
                }
            }
        }

        internal IEnumerable<Parameter> ToParameters()
        {
            var req = new RestRequest();

            req.AddParameter("user", _encEmail);
            foreach (var prop in this.GetType().GetProperties().Where(p => p.Name != "Email"))
            {
                var parmName = String.Format("{0}_{1}", _encEmail, prop.Name.ToLower());
                object value = prop.GetValue(this, null);
                if (value is bool)
                {
                    if ((bool)value)
                        req.AddParameter(parmName, 1);
                }
                else
                    req.AddParameter(parmName, value);
            }
            req.AddParameter(_encEmail + "_language", "en");  // Assume English
            return req.Parameters;
        }
    }
}
