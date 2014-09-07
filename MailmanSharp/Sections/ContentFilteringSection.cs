using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    public enum FilterActionOption { Discard, Reject, ForwardToListOwner, Preserve }

    [Path("contentfilter")]
    [Order(12)]
    public class ContentFilteringSection: SectionBase
    {
        public bool FilterContent { get; set; }
        public List<string> FilterMimeTypes { get; set; }
        public List<string> PassMimeTypes { get; set; }
        public List<string> FilterFilenameExtensions { get; set; }
        public List<string> PassFilenameExtensions { get; set; }
        public bool CollapseAlternatives { get; set; }
        public bool ConvertHtmlToPlaintext { get; set; }
        public FilterActionOption FilterAction { get; set; }

        public ContentFilteringSection(MailmanList list) : base(list) { }
    }
}
