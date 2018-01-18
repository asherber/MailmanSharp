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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public static class HtmlAgilityPackExtensions
    {
        // SelectNodes will return null if nothing is found, which kills further 
        // LINQ queries. So this returns an empty collection instead.
        public static HtmlNodeCollection SafeSelectNodes(this HtmlNode node, string xpath)
        {
            var result = node.SelectNodes(xpath);
            return result ?? new HtmlNodeCollection(null);
        }

        public static HtmlDocument GetHtmlDocument(this string content)
        {
            var doc = new HtmlDocument()
            {
                OptionFixNestedTags = true
            };

            if (!String.IsNullOrEmpty(content))
                doc.LoadHtml(content);
            return doc;
        }

        public static object GetInputValue(this HtmlDocument doc, PropertyInfo prop)
        {
            return doc.GetInputValue(prop.Name.Decamel());
        }

        public static object GetInputValue(this HtmlDocument doc, string name)
        {
            string xpath = String.Format("//input[@name='{0}']", name);
            var node = doc.DocumentNode.SafeSelectNodes(xpath).FirstOrDefault();

            return node?.Attributes["value"].Value;
        }

        public static object GetInputStringValue(this HtmlDocument doc, PropertyInfo prop)
        {
            return doc.GetInputValue(prop);
        }

        public static object GetInputIntValue(this HtmlDocument doc, PropertyInfo prop)
        {
            var val = doc.GetInputValue(prop);
            return val != null ? (object)ushort.Parse(val.ToString()) : null;
        }

        public static object GetInputDoubleValue(this HtmlDocument doc, PropertyInfo prop)
        {
            var val = doc.GetInputValue(prop);
            return val != null ? (object)double.Parse(val.ToString()) : null;
        }

        public static List<string> GetTextAreaListValue(this HtmlDocument doc, PropertyInfo prop)
        {
            return doc.GetTextAreaListValue(prop.Name.Decamel());
        }

        public static List<string> GetTextAreaListValue(this HtmlDocument doc, string name)
        {
            var text = doc.GetTextAreaStringValue(name);
            if (text == null)
                return new List<string>();
            else
                return text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
        }

        public static string GetTextAreaStringValue(this HtmlDocument doc, PropertyInfo prop)
        {
            return doc.GetTextAreaStringValue(prop.Name.Decamel());
        }

        public static string GetTextAreaStringValue(this HtmlDocument doc, string name)
        {
            string xpath = String.Format("//textarea[@name='{0}']", name);
            var node = doc.DocumentNode.SafeSelectNodes(xpath).FirstOrDefault();

            var result = node?.InnerText;
            if (String.IsNullOrEmpty(node?.InnerText))
                return null;
            else
                return result;
        }

        public static object GetInputBoolValue(this HtmlDocument doc, PropertyInfo prop)
        {
            var dname = prop.Name.Decamel();
            string xpath = String.Format("//input[@name='{0}' and @checked]", dname);
            var node = doc.DocumentNode.SafeSelectNodes(xpath).SingleOrDefault();

            return node != null ? (object)(node.Attributes["value"].Value == "1") : null;
        }

        public static object GetInputEnumValue(this HtmlDocument doc, PropertyInfo prop)
        {
            return doc.GetInputEnumValue(prop.Name.Decamel(), prop.PropertyType);
        }

        public static object GetInputEnumValue(this HtmlDocument doc, string name, Type enumType)
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
                    if (!Enum.IsDefined(enumType, enumVal))
                        throw new ArgumentException($"{enumVal} is not a defined value for enum type {enumType}");
                    result |= (int)enumVal;
                }
                return Enum.ToObject(enumType, result);
            }
            else
                return null;
        }

        public static T GetInputEnumValue<T>(this HtmlDocument doc, string name) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
            var obj = doc.GetInputEnumValue(name, typeof(T));
            if (obj != null)
                return (T)obj;
            else
                throw new ArgumentOutOfRangeException(String.Format("Value {0} not found", name));
        }
    }
}
