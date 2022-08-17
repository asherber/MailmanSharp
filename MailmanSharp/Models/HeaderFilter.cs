/**
 * Copyright 2014-2022 Aaron Sherber
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public enum HeaderFilterAction { Defer = 0, Hold = 7, Reject = 2, Discard = 3, Accept = 6 }

    [DebuggerDisplay("Action = {Action}")]
    public class HeaderFilterRule
    {
        /// <summary>
        /// List of regexes to match against the headers of a message.
        /// </summary>
        public List<string> Regexes { get; set; }
        /// <summary>
        /// Action to take if a message matches.
        /// </summary>
        public HeaderFilterAction Action { get; set; }

        public HeaderFilterRule() { }

        public HeaderFilterRule(List<string> regexes, HeaderFilterAction action)
        {
            this.Regexes = regexes;
            this.Action = action;
        }
    }
}
