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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        /// <summary>
        /// Current configuration of object as JSON.
        /// </summary>
        [JsonIgnore, MailmanIgnore]
        public string CurrentConfig => new JObject(GetCurrentConfigJProperty()).ToString();

        private string SectionName => this.GetType().Name.Replace("Section", "");
        private static JsonSerializer Serializer = new MailmanJsonSerializer();
        

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

        /// <summary>
        /// Read properties for this section from Mailman.
        /// </summary>
        /// <returns></returns>
        public virtual async Task ReadAsync()
        {
            var docs = await FetchHtmlDocumentsAsync().ConfigureAwait(false);
            var unignoredProps = _props.Unignored();

            foreach (var kvp in docs)
            {
                var propsToRead = docs.Count == 1 ? unignoredProps : unignoredProps.ForPath(kvp.Key);
                ResetProperties(propsToRead);

                var doc = kvp.Value;
                foreach (var prop in propsToRead)
                {
                    var type = prop.PropertyType;
                    if (type == typeof(string))
                    {
                        var val = doc.GetInputStringValue(prop) ?? doc.GetTextAreaStringValue(prop);
                        prop.SetValue(this, val);
                    }
                    else if (type == typeof(ushort?))
                        prop.SetValue(this, doc.GetInputIntValue(prop));
                    else if (type == typeof(double?))
                        prop.SetValue(this, doc.GetInputDoubleValue(prop));
                    else if (type == typeof(bool?))
                        prop.SetValue(this, doc.GetInputBoolValue(prop));
                    else if (Nullable.GetUnderlyingType(type)?.IsEnum == true)
                        prop.SetValue(this, doc.GetInputEnumValue(prop));
                    else if (type == typeof(List<string>))
                        prop.SetValue(this, doc.GetTextAreaListValue(prop));
                }
            }

            DoAfterRead(docs);
        }

        /// <summary>
        /// Write properties for this section to Mailman.
        /// </summary>
        /// <returns></returns>
        public virtual async Task WriteAsync()
        {
            var unignoredProps = _props.Unignored();
            var client = this.GetClient();

            foreach (var path in _paths)
            {
                var req = new RestRequest(Method.POST);
                var propsToWrite = _paths.Count == 1 ? unignoredProps : unignoredProps.ForPath(path);
                    
                foreach (var prop in propsToWrite)
                {
                    if (Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum == true)
                    {
                        foreach (var val in prop.GetEnumValues(this))
                            req.AddParameter(prop.Name.Decamel(), val);
                    }
                    else
                        req.AddParameter(prop.Name.Decamel(), prop.GetSimpleValue(this));
                }

                DoBeforeFinishWrite(req);
                await client.ExecuteAdminRequestAsync(path, req).ConfigureAwait(false);
            }
        }

        protected virtual void DoAfterRead(Dictionary<string, HtmlDocument> docs) { }
        protected virtual void DoBeforeFinishWrite(RestRequest req) { }

        
        internal virtual JProperty GetCurrentConfigJProperty()
        {
            var allProperties = JToken.FromObject(this, Serializer);
            return new JProperty(SectionName, allProperties);
        }

        /// <summary>
        /// Load configuration for this section from JSON string.
        /// </summary>
        /// <param name="json"></param>
        public void LoadConfig(string json)
        {
            var obj = JObject.Parse(json);
            obj.CheckObjectName(this.SectionName);

            var properties = obj.SelectToken(this.SectionName);
            LoadConfig(properties);
        }

        internal virtual void LoadConfig(JToken properties)
        {
            JsonConvert.PopulateObject(properties.ToString(), this);
        }

        protected async Task<Dictionary<string, HtmlDocument>> FetchHtmlDocumentsAsync()
        {
            var result = new Dictionary<string, HtmlDocument>();
            var client = this.GetClient();  // to avoid unneccessary cloning
            foreach (var path in _paths)
            {
                var resp = await client.ExecuteAdminRequestAsync(Method.GET, path).ConfigureAwait(false);
                var doc = resp.Content.GetHtmlDocument();
                result.Add(path, doc);
            }
            return result;
        }

        protected void ResetProperties(IEnumerable<PropertyInfo> props)
        {
            foreach (var prop in props.Where(p => p.CanWrite))
            {
                var type = prop.PropertyType;
                if (type == typeof(List<string>))
                    prop.SetValue(this, new List<string>());
                else if (type == typeof(string) || Nullable.GetUnderlyingType(type) != null)
                    prop.SetValue(this, null);
            }
        }
    }
}
