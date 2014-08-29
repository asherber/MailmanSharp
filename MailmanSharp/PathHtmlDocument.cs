using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public class PathHtmlDocument: HtmlAgilityPack.HtmlDocument
    {
        public string Path { get; set; }
    }
}
