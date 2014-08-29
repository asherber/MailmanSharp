using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MailmanSharp.Sections
{
    [Path("")]
    public abstract class SectionBase
    {
        protected MailmanClient _client;
        protected string _path;

        public SectionBase(MailmanList list)
        {
            _client = list.Client;

            var pathAttribute = (PathAttribute)this.GetType().GetCustomAttributes(typeof(PathAttribute), false).First();
            _path = pathAttribute.Value;
        }

        public virtual void Read()
        {
            var doc = GetHtmlDocument();
            var props = this.GetType().GetProperties();
            
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(string))
                    SetPropValue(prop, GetNodeStringValue(doc, prop));
                else if (prop.PropertyType == typeof(ushort))
                    SetPropValue(prop, GetNodeIntValue(doc, prop));
                else if (prop.PropertyType == typeof(bool))
                    SetPropValue(prop, GetNodeBoolValue(doc, prop));
                else if (prop.PropertyType.IsSubclassOf(typeof(Enum)))
                    SetPropValue(prop, GetNodeEnumValue(doc, prop));
                else if (prop.PropertyType == typeof(List<string>))
                    SetPropValue(prop, GetNodeListValue(doc, prop));
            }
        }

        public virtual void Write()
        {
            var props = this.GetType().GetProperties();
            var req = new RestRequest();

            foreach (var prop in props)
            {
                if (prop.PropertyType.IsSubclassOf(typeof(Enum)))
                {
                    foreach (var val in GetPropertyEnumValues(prop))
                        req.AddParameter(prop.Name.Decamel(), val);
                } 
                else
                    req.AddParameter(prop.Name.Decamel(), GetPropertyObjectValue(prop));
            }
            
            _client.ExecuteAdminRequest(_path, req);
        }

        private object GetPropertyObjectValue(PropertyInfo prop)
        {
            var val = prop.GetValue(this, null);
            if (prop.PropertyType == typeof(bool))
                return Convert.ToInt32(val);
            if (prop.PropertyType == typeof(List<string>))
                return String.Join("\n", (List<string>)val);
            else
                return val;
        }

        private IEnumerable<object> GetPropertyEnumValues(PropertyInfo prop)
        {
            var result = new List<object>();
            var val = prop.GetValue(this, null);

            if (prop.PropertyType.GetCustomAttributes(typeof(FlagsAttribute), false).Any())
            {
                var vals = val.ToString().ToLower().Split(new string[] { ", " }, StringSplitOptions.None);
                result.AddRange(vals);
            }
            else
            {
                result.Add((int)val);
            }
            
            return result;
        }

        
        

        private void AddToken(RestRequest req)
        {
            var doc = GetHtmlDocument();
            var node = doc.DocumentNode.SafeSelectNodes("//input[@name='csrf_token']").SingleOrDefault();
            if (node != default(HtmlNode))
                req.AddParameter("csrf_token", node.GetAttributeValue("value", null));
        }

        protected HtmlDocument GetHtmlDocument()
        {
            var resp = _client.ExecuteAdminRequest(_path);
            var result = new HtmlDocument();
            result.LoadHtml(resp.Content);
            return result;
        }

        #region Reading helpers
        protected void SetPropValue(PropertyInfo prop, object value)
        {
            prop.SetValue(this, value, null);
        }

        protected string GetNodeValue(HtmlDocument doc, PropertyInfo prop)
        {
            var dname = prop.Name.Decamel();
            string xpath = String.Format("//input[@name='{0}']", dname);
            var node = doc.DocumentNode.SafeSelectNodes(xpath).FirstOrDefault();
            
            if (node != default(HtmlNode))
                return node.GetAttributeValue("value", null);
            else
                return null;
        }

        protected string GetNodeStringValue(HtmlDocument doc, PropertyInfo prop)
        {
            return GetNodeValue(doc, prop);
        }

        protected ushort GetNodeIntValue(HtmlDocument doc, PropertyInfo prop)
        {
            return ushort.Parse(GetNodeValue(doc, prop));
        }

        protected List<string> GetNodeListValue(HtmlDocument doc, PropertyInfo prop)
        {
            var dname = prop.Name.Decamel();
            string xpath = String.Format("//textarea[@name='{0}']", dname);
            var node = doc.DocumentNode.SafeSelectNodes(xpath).FirstOrDefault();

            if (node != default(HtmlNode))
            {
                if (String.IsNullOrEmpty(node.InnerText))
                    return new List<string>();
                else
                    return node.InnerText.Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
            }
            return null;
        }

        protected bool GetNodeBoolValue(HtmlDocument doc, PropertyInfo prop)
        {
            var dname = prop.Name.Decamel();
            string xpath = String.Format("//input[@name='{0}' and @checked]", dname);
            var node = doc.DocumentNode.SafeSelectNodes(xpath).SingleOrDefault();
            
            if (node != default(HtmlNode))
                return Convert.ToBoolean(node.GetAttributeValue("value", 0));
            else
                return false;
        }

        protected object GetNodeEnumValue(HtmlDocument doc, PropertyInfo prop)
        {
            var dname = prop.Name.Decamel();
            string xpath = String.Format("//input[@name='{0}' and @checked]", dname);
            var nodes = doc.DocumentNode.SafeSelectNodes(xpath);

            int result = 0;
            foreach (var node in nodes)
            {
                var val = node.GetAttributeValue("value", null); 
                var enumVal = Enum.Parse(prop.PropertyType, val, true);
                result |= (int)enumVal;
            }
            return Enum.ToObject(prop.PropertyType, result);
        }
        #endregion

    }
}
