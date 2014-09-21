using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    // Use this instead of the base class to make sure
    // that we always fix nested tags and can store a path.
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
