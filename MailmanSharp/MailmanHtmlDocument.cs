using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public class MailmanHtmlDocument: HtmlAgilityPack.HtmlDocument
    {
        public string Path { get; set; }

        public MailmanHtmlDocument()
        {
            OptionFixNestedTags = true;
        }

        public MailmanHtmlDocument(string path): this()
        {
            this.Path = path;
        }
    }
}
