using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace MailmanSharp
{
    [Path("")]
    public abstract class SectionBase
    {
        protected MailmanList _list;
        protected HashSet<string> _paths = new HashSet<string>();
        protected MailmanClient Client { get { return _list.Client.Clone(); } }
        [Ignore]
        public string CurrentConfig { get { return GetCurrentConfig(); } }
        
        public SectionBase(MailmanList list)
        {
            _list = list;
            
            // Start with path on the class
            var basePath = GetPathValue(this.GetType().GetCustomAttributes(false));

            // Now see if we have subpaths on properties
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                var subPath = GetPathValue(prop.GetCustomAttributes(false));
                if (subPath != null)
                    _paths.Add(String.Format("{0}/{1}", basePath, subPath));
            }
            if (!_paths.Any())
                _paths.Add(basePath);

            // Initialize any reference types
            foreach (var prop in props.Where(p => p.CanWrite))
            {
                if (prop.PropertyType.GetConstructor(Type.EmptyTypes) != null)
                    prop.SetValue(this, Activator.CreateInstance(prop.PropertyType, null), null);
                else if (prop.PropertyType == typeof(string))
                    prop.SetValue(this, "", null);
            }

        }

        private string GetPathValue(object[] attributes)
        {
            var att = attributes.OfType<PathAttribute>().FirstOrDefault();
            return att != null ? att.Value : null;
        }

        public virtual void Read()
        {
            var docs = GetHtmlDocuments();
            var props = GetUnignoredProps(this.GetType());

            foreach (var doc in docs)
            {
                var propsToRead = docs.Count == 1 ? props : GetPropsForPath(props, doc.Path);
                foreach (var prop in propsToRead)
                {
                    if (prop.PropertyType == typeof(string))
                        SetPropValue(prop, GetNodeStringValue(doc, prop));
                    else if (prop.PropertyType == typeof(ushort))
                        SetPropValue(prop, GetNodeIntValue(doc, prop));
                    else if (prop.PropertyType == typeof(double))
                        SetPropValue(prop, GetNodeDoubleValue(doc, prop));
                    else if (prop.PropertyType == typeof(bool))
                        SetPropValue(prop, GetNodeBoolValue(doc, prop));
                    else if (prop.PropertyType.IsSubclassOf(typeof(Enum)))
                        SetPropValue(prop, GetNodeEnumValue(doc, prop));
                    else if (prop.PropertyType == typeof(List<string>))
                        SetPropValue(prop, GetNodeListValue(doc, prop));
                }
            }

            DoAfterRead(docs);
        }

        public virtual void Write()
        {
            var props = GetUnignoredProps(this.GetType());
            var client = this.Client;

            foreach (var path in _paths)
            {
                var req = new RestRequest();
                var propsToWrite = _paths.Count == 1 ? props : GetPropsForPath(props, path);
                    
                foreach (var prop in propsToWrite)
                {
                    if (prop.PropertyType.IsSubclassOf(typeof(Enum)))
                    {
                        foreach (var val in GetPropertyEnumValues(prop))
                            req.AddParameter(prop.Name.Decamel(), val);
                    }
                    else
                        req.AddParameter(prop.Name.Decamel(), GetPropertyObjectValue(prop));
                }

                DoBeforeFinishWrite(req);
                client.ExecuteAdminRequest(path, req);
            }
        }

        protected virtual void DoAfterRead(List<MailmanHtmlDocument> docs) { }
        protected virtual void DoBeforeFinishWrite(RestRequest req) { }

        protected IEnumerable<PropertyInfo> GetPropsForPath(IEnumerable<PropertyInfo> props, string path)
        {
            return props.Where(p => p.GetCustomAttributes(false).OfType<PathAttribute>().Any(a => path.Contains(a.Value)));
        }

        protected IEnumerable<PropertyInfo> GetUnignoredProps(Type type)
        {
            var props = type.GetProperties();
            return props.Where(p => !p.GetCustomAttributes(false).OfType<IgnoreAttribute>().Any());
        }

        internal virtual string GetCurrentConfig()
        {
            var result = new XElement(GetSectionName());
            var props = GetUnignoredProps(this.GetType());

            foreach (var prop in props)
            {
                var val = prop.GetValue(this, null);
                if (val is List<string>)
                    val = String.Join("\n", (List<string>)val);                    
                    
                result.Add(new XElement(prop.Name, val));
            }
            return result.ToString();
        }

        public void LoadConfig(string xml)
        {
            var root = XElement.Parse(xml);
            root.CheckElementName(GetSectionName());

            var props = GetUnignoredProps(this.GetType());
            foreach (var prop in props)
            {
                var el = root.Element(prop.Name);
                //if (el != null && !String.IsNullOrEmpty(el.Value))
                if (el != null)
                {
                    if (prop.PropertyType == typeof(bool))
                        prop.SetValue(this, Convert.ToBoolean(el.Value), null);
                    else if (prop.PropertyType == typeof(ushort))
                        prop.SetValue(this, Convert.ToUInt16(el.Value), null);
                    else if (prop.PropertyType == typeof(double))
                        prop.SetValue(this, Convert.ToDouble(el.Value), null);
                    else if (prop.PropertyType == typeof(List<string>))
                    {
                        var list = el.Value.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
                        prop.SetValue(this, list, null);
                    }
                    else if (prop.PropertyType.IsSubclassOf(typeof(Enum)))
                        prop.SetValue(this, Enum.Parse(prop.PropertyType, el.Value, true), null);
                    else
                        prop.SetValue(this, el.Value, null);
                }
            }
        }

        private string GetSectionName()
        {
            return this.GetType().Name.Replace("Section", "");
        }

        private object GetPropertyObjectValue(PropertyInfo prop)
        {
            var val = prop.GetValue(this, null);
            if (prop.PropertyType == typeof(bool))
                return Convert.ToInt32(val);
            else if (prop.PropertyType == typeof(List<string>))
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

        protected List<MailmanHtmlDocument> GetHtmlDocuments()
        {
            var result = new List<MailmanHtmlDocument>();
            var client = this.Client;  // to avoid unneccessary cloning
            foreach (var path in _paths)
            {
                var resp = client.ExecuteAdminRequest(path);
                var doc = new MailmanHtmlDocument(path);
                doc.LoadHtml(resp.Content);
                result.Add(doc);
            }
            return result;
        }

        #region Reading helpers
        protected void SetPropValue(PropertyInfo prop, object value)
        {
            if (value != null)
                prop.SetValue(this, value, null);
        }

        protected object GetNodeValue(HtmlDocument doc, PropertyInfo prop)
        {
            var dname = prop.Name.Decamel();
            string xpath = String.Format("//input[@name='{0}']", dname);
            var node = doc.DocumentNode.SafeSelectNodes(xpath).FirstOrDefault();

            return node != null ? node.GetAttributeValue("value", null) : null;
        }

        protected object GetNodeStringValue(HtmlDocument doc, PropertyInfo prop)
        {
            return GetNodeValue(doc, prop);
        }

        protected object GetNodeIntValue(HtmlDocument doc, PropertyInfo prop)
        {
            var val = GetNodeValue(doc, prop);
            return val != null ? (object)ushort.Parse(val.ToString()) : null;
        }

        protected object GetNodeDoubleValue(HtmlDocument doc, PropertyInfo prop)
        {
            var val = GetNodeValue(doc, prop);
            return val != null ? (object)double.Parse(val.ToString()) : null;
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

        protected object GetNodeBoolValue(HtmlDocument doc, PropertyInfo prop)
        {
            var dname = prop.Name.Decamel();
            string xpath = String.Format("//input[@name='{0}' and @checked]", dname);
            var node = doc.DocumentNode.SafeSelectNodes(xpath).SingleOrDefault();

            return node != null ? (object)Convert.ToBoolean(node.GetAttributeValue("value", 0)) : null;
        }

        protected object GetNodeEnumValue(HtmlDocument doc, PropertyInfo prop)
        {
            var dname = prop.Name.Decamel();
            string xpath = String.Format("//input[@name='{0}' and @checked]", dname);
            var nodes = doc.DocumentNode.SafeSelectNodes(xpath);

            if (nodes.Any())
            {
                int result = 0;
                foreach (var node in nodes)
                {
                    var val = node.GetAttributeValue("value", null);
                    var enumVal = Enum.Parse(prop.PropertyType, val, true);
                    result |= (int)enumVal;
                }
                return Enum.ToObject(prop.PropertyType, result);
            }
            else
                return null;
        }
        #endregion

    }
}
