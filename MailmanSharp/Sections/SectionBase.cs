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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MailmanSharp
{
    [Path("")]
    public abstract class SectionBase: ISectionBase
    {
        protected MailmanList _list;
        protected HashSet<string> _paths = new HashSet<string>();

        protected static IDictionary<Type, IEnumerable<PropertyInfo>> _propsDict = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        protected IEnumerable<PropertyInfo> _props => _propsDict[this.GetType()];
        
        [Ignore]
        public string CurrentConfig { get { return GetCurrentConfig(); } }

        private string SectionName => this.GetType().Name.Replace("Section", "");
        

        internal SectionBase(MailmanList list)
        {
            _list = list ?? throw new ArgumentNullException("list");
            
            if (!_propsDict.ContainsKey(this.GetType()))
            {
                _propsDict[this.GetType()] = this.GetType().GetProperties();
            }
            
            // Start with path on the class
            var basePath = GetPathValue(this.GetType().GetCustomAttributes(false));

            // Now see if we have subpaths on properties
            foreach (var prop in _props)
            {
                var subPath = GetPathValue(prop.GetCustomAttributes(false));
                if (subPath != null)
                    _paths.Add(String.Format("{0}/{1}", basePath, subPath));
            }
            if (!_paths.Any())
                _paths.Add(basePath);
        }

        protected IMailmanClient GetClient()
        {
            return _list.InternalClient.Clone();
        }

        private string GetPathValue(object[] attributes)
        {
            var att = attributes.OfType<PathAttribute>().FirstOrDefault();
            return att?.Value;
        }

        public virtual async Task ReadAsync()
        {
            var docs = await FetchHtmlDocumentsAsync().ConfigureAwait(false);
            var unignoredProps = _props.GetUnignored();

            foreach (var kvp in docs)
            {
                var propsToRead = docs.Count == 1 ? unignoredProps : unignoredProps.GetForPath(kvp.Key);
                var doc = kvp.Value;
                foreach (var prop in propsToRead)
                {
                    if (prop.PropertyType == typeof(string))
                        prop.SetValueIfNotNull(this, doc.GetNodeStringValue(prop));
                    else if (prop.PropertyType == typeof(ushort))
                        prop.SetValueIfNotNull(this, doc.GetNodeIntValue(prop));
                    else if (prop.PropertyType == typeof(double))
                        prop.SetValueIfNotNull(this, doc.GetNodeDoubleValue(prop));
                    else if (prop.PropertyType == typeof(bool))
                        prop.SetValueIfNotNull(this, doc.GetNodeBoolValue(prop));
                    else if (prop.PropertyType.IsSubclassOf(typeof(Enum)))
                        prop.SetValueIfNotNull(this, doc.GetNodeEnumValue(prop));
                    else if (prop.PropertyType == typeof(List<string>))
                        prop.SetValueIfNotNull(this, doc.GetNodeListValue(prop));
                }
            }

            DoAfterRead(docs);
        }

        public virtual async Task WriteAsync()
        {
            var unignoredProps = _props.GetUnignored();
            var client = this.GetClient();

            foreach (var path in _paths)
            {
                var req = new RestRequest();
                var propsToWrite = _paths.Count == 1 ? unignoredProps : unignoredProps.GetForPath(path);
                    
                foreach (var prop in propsToWrite)
                {
                    if (prop.PropertyType.IsSubclassOf(typeof(Enum)))
                    {
                        foreach (var val in prop.GetEnumValues(this))
                            req.AddParameter(prop.Name.Decamel(), val);
                    }
                    else
                        req.AddParameter(prop.Name.Decamel(), prop.GetObjectValue(this));
                }

                DoBeforeFinishWrite(req);
                await client.ExecutePostAdminRequestAsync(path, req).ConfigureAwait(false);
            }
        }

        protected virtual void DoAfterRead(Dictionary<string, HtmlDocument> docs) { }
        protected virtual void DoBeforeFinishWrite(RestRequest req) { }

        
        internal virtual string GetCurrentConfig()
        {
            var result = new XElement(this.SectionName);
            var unignoredProps = _props.GetUnignored();

            foreach (var prop in unignoredProps)
            {
                var val = prop.GetValue(this);
                if (val is List<string>)
                    val = ((List<string>)val).Cat();
                    
                result.Add(new XElement(prop.Name, val));
            }
            return result.ToString();
        }

        public void LoadConfig(string xml)
        {
            var root = XElement.Parse(xml);
            root.CheckElementName(this.SectionName);

            var unignoredProps = _props.GetUnignored();
            foreach (var prop in unignoredProps)
            {
                var el = root.Element(prop.Name);
                //if (el != null && !String.IsNullOrEmpty(el.Value))
                if (el != null)
                {
                    if (prop.PropertyType == typeof(bool))
                        prop.SetValue(this, Convert.ToBoolean(el.Value));
                    else if (prop.PropertyType == typeof(ushort))
                        prop.SetValue(this, Convert.ToUInt16(el.Value));
                    else if (prop.PropertyType == typeof(double))
                        prop.SetValue(this, Convert.ToDouble(el.Value));
                    else if (prop.PropertyType == typeof(List<string>))
                    {
                        var list = el.Value.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
                        prop.SetValue(this, list);
                    }
                    else if (prop.PropertyType.IsSubclassOf(typeof(Enum)))
                        prop.SetValue(this, Enum.Parse(prop.PropertyType, el.Value, true));
                    else
                        prop.SetValue(this, el.Value);
                }
            }
        }

        protected async Task<Dictionary<string, HtmlDocument>> FetchHtmlDocumentsAsync()
        {
            var result = new Dictionary<string, HtmlDocument>();
            var client = this.GetClient();  // to avoid unneccessary cloning
            foreach (var path in _paths)
            {
                var resp = await client.ExecuteGetAdminRequestAsync(path).ConfigureAwait(false);
                var doc = resp.Content.GetHtmlDocument();
                result.Add(path, doc);
            }
            return result;
        }
    }
}
