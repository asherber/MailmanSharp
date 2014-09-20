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
    public enum NoMailReason { None, Unknown, Bounce, User, Administrator }

    public class Member
    {
        [Ignore]
        public string Email { get; internal set; }
        public string RealName { get; set; }
        public bool Mod { get; set; }
        public bool Hide { get; set; }
        public bool NoMail { get; set; }
        [Ignore]
        public NoMailReason NoMailReason { get; set; }
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

            foreach (var prop in this.GetType().GetUnignoredProps())
            {
                var name = String.Format("{0}_{1}", _encEmail, prop.Name.ToLower());
                var thisNode = nodes.SingleOrDefault(n => n.GetAttributeValue("name", null) == name);
                if (thisNode != null)
                {
                    var val = thisNode.GetAttributeValue("value", null);
                    if (prop.PropertyType == typeof(string))
                        prop.SetValue(this, val, null);
                    else if (prop.PropertyType == typeof(bool))
                        prop.SetValue(this, val == "on", null);
                }
            }

            if (this.NoMail)
            {
                this.NoMailReason = NoMailReason.Unknown;
                string name = _encEmail + "_nomail";
                var node = nodes.SingleOrDefault(n => n.GetAttributeValue("name", null) == name);
                if (node != null)
                {
                    string reason = node.NextSibling.InnerText;
                    switch (reason)
                    {
                        case "[A]": 
                            this.NoMailReason = NoMailReason.Administrator;
                            break;
                        case "[B]":
                            this.NoMailReason = NoMailReason.Bounce;
                            break;
                        case "[U]":
                            this.NoMailReason = NoMailReason.User;
                            break;
                    }
                }
            }
        }

        internal IEnumerable<Parameter> ToParameters()
        {
            var req = new RestRequest();

            req.AddParameter("user", _encEmail);
            foreach (var prop in this.GetType().GetUnignoredProps())
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
