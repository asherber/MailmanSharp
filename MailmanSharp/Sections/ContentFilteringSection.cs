using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    public enum FilterActionOption { Discard, Reject, ForwardToListOwner, Preserve }

    [Path("contentfilter")]
    public class ContentFilteringSection: SectionBase
    {
        public bool FilterContent { get; set; }
        public string FilterMimeTypes { get; set; }
        public string PassMimeTypes { get; set; }
        public string FilterFilenameExtensions { get; set; }
        public string PassFilenameExtensions { get; set; }
        public bool CollapseAlternatives { get; set; }
        public bool ConvertHtmlToPlaintext { get; set; }
        public FilterActionOption FilterAction { get; set; }

        public ContentFilteringSection(MailmanList list) : base(list) { }
    }
}
