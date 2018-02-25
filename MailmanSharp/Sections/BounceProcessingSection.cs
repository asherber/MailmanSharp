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
    [Path("bounce")]
    [Order(8)]
    public class BounceProcessingSection: SectionBase
    {
        /// <summary>
        /// Should Mailman perform automatic bounce processing?
        /// </summary>
        public bool? BounceProcessing { get; set; }
        /// <summary>
        /// The maximum member bounce score before the member's subscription is disabled. 
        /// </summary>
        public double? BounceScoreThreshold { get; set; }
        /// <summary>
        /// The number of days after which a member's bounce information is discarded, 
        /// if no new bounces have been received in the interim. 
        /// </summary>
        public ushort? BounceInfoStaleAfter { get; set; }
        /// <summary>
        /// How many Your Membership Is Disabled warnings a disabled member should get before 
        /// their address is removed from the mailing list. Set to 0 to immediately remove 
        /// an address from the list once their bounce score exceeds the threshold. 
        /// </summary>
        public ushort? BounceYouAreDisabledWarnings { get; set; }
        /// <summary>
        /// The number of days between sending the Your Membership Is Disabled warnings. 
        /// </summary>
        public ushort? BounceYouAreDisabledWarningsInterval { get; set; }
        /// <summary>
        ///  Should Mailman send the list owner any bounce messages that failed to be 
        ///  detected by the bounce processor? Yes is recommended.
        /// </summary>
        public bool? BounceUnrecognizedGoesToListOwner { get; set; }
        /// <summary>
        /// Should Mailman notify the list owner when bounces cause a member's 
        /// bounce score to be incremented?
        /// </summary>
        public bool? BounceNotifyOwnerOnBounceIncrement { get; set; }
        /// <summary>
        /// Should Mailman notify the list owner when bounces cause a member's subscription to be disabled?
        /// </summary>
        public bool? BounceNotifyOwnerOnDisable { get; set; }
        /// <summary>
        /// Should Mailman notify the list owner when bounces cause a member to be unsubscribed?
        /// </summary>
        public bool? BounceNotifyOwnerOnRemoval { get; set; }

        internal BounceProcessingSection(MailmanList list) : base(list) { }
    }
}
