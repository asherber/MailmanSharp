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
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public enum AutorespondRequestsOption { No, YesWithDiscard, YesWithForward }

    [Path("autoreply")]
    [Order(11)]
    public class AutoResponderSection: SectionBase
    {
        /// <summary>
        /// Should Mailman send an auto-response to mailing list posters?
        /// </summary>
        public bool? AutorespondPostings { get; set; }
        /// <summary>
        /// Auto-response text to send to mailing list posters.
        /// </summary>
        public string AutoresponsePostingsText { get; set; }
        /// <summary>
        /// Should Mailman send an auto-response to emails sent to the -owner address?
        /// </summary>
        public bool? AutorespondAdmin { get; set; }
        /// <summary>
        /// Auto-response text to send to -owner emails.
        /// </summary>
        public string AutoresponseAdminText { get; set; }
        /// <summary>
        /// Should Mailman send an auto-response to emails sent to the -request address? 
        /// </summary>
        public AutorespondRequestsOption? AutorespondRequests { get; set; }
        /// <summary>
        /// Auto-response text to send to -request emails.
        /// </summary>
        public string AutoresponseRequestText { get; set; }
        /// <summary>
        /// Number of days between auto-responses to either the mailing list or -request/-owner 
        /// address from the same poster. Set to zero (or negative) for no grace period 
        /// (i.e. auto-respond to every message).
        /// </summary>
        public ushort? AutoresponseGraceperiod { get; set; }


        internal AutoResponderSection(MailmanList list) : base(list) { }
    }
}
