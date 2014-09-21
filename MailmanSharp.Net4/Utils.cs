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
        internal static IEnumerable<PropertyInfo> GetUnignoredProps(this Type type)
        {
            var props = type.GetProperties();
            return props.Where(p => !p.GetCustomAttributes(false).OfType<IgnoreAttribute>().Any());
        }
    }
}
