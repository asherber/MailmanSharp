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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace MailmanSharp
{
    public static class MiscellaneousExtensions
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

        public static void CheckObjectName(this JObject obj, string expectedName)
        {
            if ((obj.First as JProperty).Name != expectedName)
                throw new JsonException("Incorrect root property name.");
        }

        public static int ToInt(this bool input)
        {
            return input ? 1 : 0;
        }

        public static IEnumerable<PropertyInfo> Unignored(this IEnumerable<PropertyInfo> props)
        {
            return props.Where(p => !p.GetCustomAttributes(false).OfType<IgnoreAttribute>().Any());
        }

        public static IEnumerable<PropertyInfo> ForPath(this IEnumerable<PropertyInfo> props, string path)
        {
            return props.Where(p => p.GetCustomAttributes(false).OfType<PathAttribute>().Any(a => path.Contains(a.Value)));
        }

        public static string Cat(this IEnumerable<string> strings)
        {
            return String.Join("\n", strings); 
        }

        public static object GetSimpleValue(this PropertyInfo prop, object obj)
        {
            var val = prop.GetValue(obj);
            if (prop.PropertyType == typeof(bool?))
                return Convert.ToInt32(val);
            else if (prop.PropertyType == typeof(List<string>))
                return ((List<string>)val).Cat();
            else
                return val;
        }

        public static IEnumerable<object> GetEnumValues(this PropertyInfo prop, object obj)
        {
            var result = new List<object>();
            var val = prop.GetValue(obj);

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


    }
}
