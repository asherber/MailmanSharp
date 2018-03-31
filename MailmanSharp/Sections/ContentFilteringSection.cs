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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public enum FilterActionOption { Discard, Reject, ForwardToListOwner, Preserve }

    [Path("contentfilter")]
    [Order(12)]
    public class ContentFilteringSection: SectionBase
    {
        /// <summary>
        /// Should Mailman filter the content of list traffic according to the settings in this section?
        /// </summary>
        public bool? FilterContent { get; set; }
        /// <summary>
        /// Remove message attachments that have a matching content type.
        /// </summary>
        public List<string> FilterMimeTypes { get; set; } = new List<string>();
        /// <summary>
        /// Remove message attachments that don't have a matching content type. Leave this property empty to skip this filter test.
        /// </summary>
        public List<string> PassMimeTypes { get; set; } = new List<string>();
        /// <summary>
        /// Remove message attachments that have a matching filename extension.
        /// </summary>
        public List<string> FilterFilenameExtensions { get; set; } = new List<string>();
        /// <summary>
        /// Remove message attachments that don't have a matching filename extension. Leave this property empty to skip this filter test.
        /// </summary>
        public List<string> PassFilenameExtensions { get; set; } = new List<string>();
        /// <summary>
        /// Should Mailman collapse multipart/alternative to its first part content?
        /// </summary>
        public bool? CollapseAlternatives { get; set; }
        /// <summary>
        /// Should Mailman convert text/html parts to plain text? This conversion happens after MIME attachments have been stripped.
        /// </summary>
        public bool? ConvertHtmlToPlaintext { get; set; }
        /// <summary>
        /// Action to take when a message matches the content filtering rules.
        /// </summary>
        public FilterActionOption? FilterAction { get; set; }

        internal ContentFilteringSection(MailmanList list) : base(list) { }
    }
}
