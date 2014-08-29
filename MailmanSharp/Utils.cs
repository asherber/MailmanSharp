using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MailmanSharp
{
    static class Utils
    {
        private static Regex _humpRegex = new Regex(@"\B[A-Z]");
        public static string Decamel(this string input)
        {
            return _humpRegex.Replace(input, "_$0").ToLower();
        }

        private static Regex _underscoreRegex = new Regex("(^|_)([a-z])");
        public static string Encamel(this string input)
        {
            return _underscoreRegex.Replace(input, m => m.Groups[2].Value.ToUpper());
        }

        // SelectNodes will return null if nothing is found, which kills further 
        // LINQ queries. So this returns an empty collection instead.
        public static HtmlNodeCollection SafeSelectNodes(this HtmlNode node, string xpath)
        {
            var result = node.SelectNodes(xpath);
            return result ?? new HtmlNodeCollection(null);
        }
    }
}
