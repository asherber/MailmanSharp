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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MailmanSharp
{
    public static class Utils
    {
        private static Regex _humpRegex = new Regex(@"(?<!_)\B[A-Z]");
        // CamelCaseString -> camel_case_string
        public static string Decamel(this string input)
        {
            return _humpRegex.Replace(input, "_$0").ToLower();
        }

        private static Regex _underscoreRegex = new Regex("(^|_)([a-z])");
        // underscore_string -> UnderscoreString
        public static string Encamel(this string input)
        {
            string result = _underscoreRegex.Replace(input, m => m.Groups[2].Value.ToUpper());
            if (!String.IsNullOrEmpty(input) && input[0] == '_')
                result = "_" + result;
            return result;
        }

        // SelectNodes will return null if nothing is found, which kills further 
        // LINQ queries. So this returns an empty collection instead.
        public static HtmlNodeCollection SafeSelectNodes(this HtmlNode node, string xpath)
        {
            var result = node.SelectNodes(xpath);
            return result ?? new HtmlNodeCollection(null);
        }

        public static void CheckElementName(this XElement element, string expectedName)
        {
            if (element.Name != expectedName)
                throw new XmlException("Incorrect root element name.", null, 1, 2);
        }

        public static IRestRequest AddOrSetParameter(this IRestRequest req, string name, object value)
        {
            var parms = req.Parameters.Where(p => String.Compare(p.Name, name, true) == 0);
            if (parms.Any())
                parms.First().Value = value;
            else
                req.AddParameter(name, value);

            return req;
        }

        public static int ToInt(this bool input)
        {
            return input ? 1 : 0;
        }

        // Get list of properties for type without [Ignore] attribute
        internal static IEnumerable<PropertyInfo> GetUnignoredProps(this IEnumerable<PropertyInfo> props)
        {
            return props.Where(p => !p.GetCustomAttributes(false).OfType<IgnoreAttribute>().Any());
        }

        public static string Cat(this IEnumerable<string> strings)
        {
            return String.Join("\n", strings); 
        }
    }
}
