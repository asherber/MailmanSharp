/**
 * Copyright 2014-5 Aaron Sherber
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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MailmanSharp
{
    [Path("")]
    public abstract class SectionBase
    {
        protected MailmanList _list;
        protected HashSet<string> _paths = new HashSet<string>();
        
        [Ignore]
        public string CurrentConfig { get { return GetCurrentConfig(); } }
        
        internal SectionBase(MailmanList list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

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

        protected MailmanClient GetClient()
        {
            return _list.Client.Clone();
        }

        private string GetPathValue(object[] attributes)
        {
            var att = attributes.OfType<PathAttribute>().FirstOrDefault();
            return att != null ? att.Value : null;
        }

        public virtual void Read()
        {
            var docs = FetchHtmlDocuments();
            var props = this.GetType().GetUnignoredProps();

            foreach (var kvp in docs)
            {
                var propsToRead = docs.Count == 1 ? props : GetPropsForPath(props, kvp.Key);
                var doc = kvp.Value;
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
            var props = this.GetType().GetUnignoredProps();
            var client = this.GetClient();

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
                client.ExecutePostAdminRequest(path, req);
            }
        }

        public Task ReadAsync()
        {
            return Task.Run(() => this.Read());
        }

        public Task WriteAsync()
        {
            return Task.Run(() => this.Write());
        }

        protected virtual void DoAfterRead(Dictionary<string, HtmlDocument> docs) { }
        protected virtual void DoBeforeFinishWrite(RestRequest req) { }

        protected IEnumerable<PropertyInfo> GetPropsForPath(IEnumerable<PropertyInfo> props, string path)
        {
            return props.Where(p => p.GetCustomAttributes(false).OfType<PathAttribute>().Any(a => path.Contains(a.Value)));
        }

        

        internal virtual string GetCurrentConfig()
        {
            var result = new XElement(GetSectionName());
            var props = this.GetType().GetUnignoredProps();

            foreach (var prop in props)
            {
                var val = prop.GetValue(this, null);
                if (val is List<string>)
                    val = ((List<string>)val).Cat();
                    
                result.Add(new XElement(prop.Name, val));
            }
            return result.ToString();
        }

        public void LoadConfig(string xml)
        {
            var root = XElement.Parse(xml);
            root.CheckElementName(GetSectionName());

            var props = this.GetType().GetUnignoredProps();
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
                return ((List<string>)val).Cat();
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

        protected Dictionary<string, HtmlDocument> FetchHtmlDocuments()
        {
            var result = new Dictionary<string, HtmlDocument>();
            var client = this.GetClient();  // to avoid unneccessary cloning
            foreach (var path in _paths)
            {
                var resp = client.ExecuteGetAdminRequest(path);
                var doc = GetHtmlDocument(resp.Content);
                result.Add(path, doc);
            }
            return result;
        }

        protected static HtmlDocument GetHtmlDocument(string content = null)
        {
            var doc = new HtmlDocument()
            {
                OptionFixNestedTags = true
            };

            if (!String.IsNullOrEmpty(content))
                doc.LoadHtml(content);
            return doc;
        }

        #region Reading helpers
        protected void SetPropValue(PropertyInfo prop, object value)
        {
            if (value != null)
                prop.SetValue(this, value, null);
        }

        protected object GetNodeValue(HtmlDocument doc, PropertyInfo prop)
        {
            return GetNodeValue(doc, prop.Name.Decamel());
        }

        protected object GetNodeValue(HtmlDocument doc, string name)
        {
            string xpath = String.Format("//input[@name='{0}']", name);
            var node = doc.DocumentNode.SafeSelectNodes(xpath).FirstOrDefault();

            return node != null ? node.Attributes["value"].Value : null;
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
            return GetNodeListValue(doc, prop.Name.Decamel());           
        }

        protected List<string> GetNodeListValue(HtmlDocument doc, string name)
        {
            string xpath = String.Format("//textarea[@name='{0}']", name);
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

            return node != null ? (object)(node.Attributes["value"].Value == "1") : null;
        }

        protected object GetNodeEnumValue(HtmlDocument doc, PropertyInfo prop)
        {
            return GetNodeEnumValue(doc, prop.Name.Decamel(), prop.PropertyType);
        }

        protected object GetNodeEnumValue(HtmlDocument doc, string name, Type enumType)
        {
            string xpath = String.Format("//input[@name='{0}' and @checked]", name);
            var nodes = doc.DocumentNode.SafeSelectNodes(xpath);

            if (nodes.Any())
            {
                int result = 0;
                foreach (var node in nodes)
                {
                    var val = node.Attributes["value"].Value;
                    var enumVal = Enum.Parse(enumType, val, true);
                    result |= (int)enumVal;
                }
                return Enum.ToObject(enumType, result);
            }
            else
                return null;
        }

        protected T GetNodeEnumValue<T>(HtmlDocument doc, string name) where T: struct, IConvertible 
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
            var obj = GetNodeEnumValue(doc, name, typeof(T));
            if (obj != null)
                return (T)obj;
            else
                throw new Exception(String.Format("Value {0} not found", name));
        }
        #endregion

    }
}
