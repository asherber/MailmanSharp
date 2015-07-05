/**
 * Copyright 2014-5 Aaron Sherber
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
    public enum AutorespondRequestsOption { No, YesWithDiscard, YesWithForward }

    [Path("autoreply")]
    [Order(11)]
    public class AutoResponderSection: SectionBase
    {
        public bool AutorespondPostings { get; set; }
        public List<string> AutoresponsePostingsText { get; set; }
        public bool AutorespondAdmin { get; set; }
        public List<string> AutoresponseAdminText { get; set; }
        public AutorespondRequestsOption AutorespondRequests { get; set; }
        public List<string> AutoresponseRequestText { get; set; }
        public ushort AutoresponseGraceperiod { get; set; }


        internal AutoResponderSection(MailmanList list) : base(list) { }
    }
}
