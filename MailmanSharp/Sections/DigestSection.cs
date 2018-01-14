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
        public bool Digestable { get; set; }
        public DigestIsDefaultOption DigestIsDefault { get; set; }
        public MimeIsDefaultDigestOption MimeIsDefaultDigest { get; set; }
        public ushort DigestSizeThreshhold { get; set; }
        public bool DigestSendPeriodic { get; set; }
        public List<string> DigestHeader { get; set; } = new List<string>();
        public List<string> DigestFooter { get; set; } = new List<string>();
        public DigestVolumeFrequencyOption DigestVolumeFrequency { get; set; }
        
        public Task NewVolumeAsync()
        {
            return this.GetClient().ExecutePostAdminRequestAsync(_paths.First(), "_new_volume", 1);
        }

        public Task SendDigestNowAsync()
        {
            return this.GetClient().ExecutePostAdminRequestAsync(_paths.First(), "_send_digest_now", 1);
        }

        internal DigestSection(MailmanList list) : base(list) { }
    }
}
