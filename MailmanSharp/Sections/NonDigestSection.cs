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
    public enum PersonalizeOption { No, Yes, FullPersonalization }
    
    [Path("nondigest")]
    [Order(5)]
    public class NonDigestSection: SectionBase
    {
        /// <summary>
        /// Can subscribers choose to receive mail immediately, rather than in batched digests? 
        /// </summary>
        public bool? Nondigestable { get; set; }
        /// <summary>
        /// Should Mailman personalize each non-digest delivery? 
        /// </summary>
        public PersonalizeOption? Personalize { get; set; }
        /// <summary>
        /// Header added to mail sent to regular list members.
        /// </summary>
        public string MsgHeader { get; set; }
        /// <summary>
        /// Footer added to mail sent to regular list members.
        /// </summary>
        public string MsgFooter { get; set; }
        /// <summary>
        /// Scrub attachments of regular delivery message? 
        /// </summary>
        public bool? ScrubNondigest { get; set; }
        /// <summary>
        /// Other mailing lists on this site whose members are excluded from the regular 
        /// (non-digest) delivery if those list addresses appear in a To: or Cc: header.
        /// </summary>
        public List<string> RegularExcludeLists { get; set; } = new List<string>();
        /// <summary>
        /// Ignore regular_exclude_lists of which the poster is not a member. 
        /// </summary>
        public bool? RegularExcludeIgnore { get; set; }
        /// <summary>
        /// Other mailing lists on this site whose members are included in the regular 
        /// (non-digest) delivery if those list addresses don't appear in a To: or Cc: header. 
        /// </summary>
        public List<string> RegularIncludeLists { get; set; } = new List<string>();

        internal NonDigestSection(MailmanList list) : base(list) { }
    }
}
