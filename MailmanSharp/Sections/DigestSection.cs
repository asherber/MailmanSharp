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

using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public enum DigestIsDefaultOption { Regular, Digest }
    public enum MimeIsDefaultDigestOption { Plain, Mime }
    public enum DigestVolumeFrequencyOption { Yearly, Monthly, Quarterly, Weekly, Daily }

    [Path("digest")]
    [Order(6)]
    public class DigestSection: SectionBase
    {
        /// <summary>
        /// Can list members choose to receive list traffic bunched in digests?
        /// </summary>
        public bool? Digestable { get; set; }
        /// <summary>
        /// Which delivery mode is the default for new users?
        /// </summary>
        public DigestIsDefaultOption? DigestIsDefault { get; set; }
        /// <summary>
        /// When receiving digests, which format is default?
        /// </summary>
        public MimeIsDefaultDigestOption? MimeIsDefaultDigest { get; set; }
        /// <summary>
        /// How big in Kb should a digest be before it gets sent out? 0 implies no maximum size. 
        /// </summary>
        public ushort? DigestSizeThreshhold { get; set; }
        /// <summary>
        /// Should a digest be dispatched daily when the size threshold isn't reached?
        /// </summary>
        public bool? DigestSendPeriodic { get; set; }
        /// <summary>
        /// Header added to every digest.
        /// </summary>
        public string DigestHeader { get; set; }
        /// <summary>
        /// Footer added to every digest.
        /// </summary>
        public string DigestFooter { get; set; }
        /// <summary>
        /// How often should a new digest volume be started?
        /// </summary>
        public DigestVolumeFrequencyOption? DigestVolumeFrequency { get; set; }
        
        /// <summary>
        /// Start a new digest volume.
        /// </summary>
        /// <returns></returns>
        public Task NewVolumeAsync()
        {
            return this.GetClient().ExecuteAdminRequestAsync(Method.POST, _paths.First(), ("_new_volume", 1));
        }

        /// <summary>
        /// Send the next digest right now, if it is not empty
        /// </summary>
        /// <returns></returns>
        public Task SendDigestNowAsync()
        {
            return this.GetClient().ExecuteAdminRequestAsync(Method.POST, _paths.First(), ("_send_digest_now", 1));
        }

        internal DigestSection(MailmanList list) : base(list) { }
    }
}
