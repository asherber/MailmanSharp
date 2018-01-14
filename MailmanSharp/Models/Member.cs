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

using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace MailmanSharp
{
    public enum NoMailReason { None, Unknown, Bounce, User, Administrator }

    [DebuggerDisplay("Email = {Email}, RealName = {RealName}")]
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

        protected static IEnumerable<PropertyInfo> _props;

        static Member()
        {
            _props = typeof(Member).GetProperties();
        }
        
        internal Member(HtmlNodeCollection nodes)
        {
            var firstNode = nodes.First();
            _encEmail = Regex.Replace(firstNode.Attributes["name"].Value, "_\\w*$", "");
            this.Email = HttpUtility.UrlDecode(_encEmail);

            foreach (var prop in _props.GetUnignored())
            {
                var name = String.Format("{0}_{1}", _encEmail, prop.Name.ToLower());
                var thisNode = nodes.SingleOrDefault(n => n.Attributes["name"].Value == name);
                if (thisNode != null)
                {
                    var val = thisNode.Attributes["value"].Value;
                    if (prop.PropertyType == typeof(string))
                        prop.SetValue(this, val);
                    else if (prop.PropertyType == typeof(bool))
                        prop.SetValue(this, val == "on");
                }
            }

            if (this.NoMail)
            {
                this.NoMailReason = NoMailReason.Unknown;
                string name = _encEmail + "_nomail";
                var node = nodes.SingleOrDefault(n => n.Attributes["name"].Value == name);
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
            foreach (var prop in _props.GetUnignored())
            {
                var parmName = String.Format("{0}_{1}", _encEmail, prop.Name.ToLower());
                object value = prop.GetValue(this);
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
