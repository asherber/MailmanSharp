/**
 * Copyright 2014 Aaron Sherber
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
        public bool Nondigestable { get; set; }
        public PersonalizeOption Personalize { get; set; }
        public List<string> MsgHeader { get; set; }
        public List<string> MsgFooter { get; set; }
        public bool ScrubNondigest { get; set; }
        public List<string> RegularExcludeLists { get; set; }
        public bool RegularExcludeIgnore { get; set; }
        public List<string> RegularIncludeLists { get; set; }

        public NonDigestSection(MailmanList list) : base(list) { }
    }
}
