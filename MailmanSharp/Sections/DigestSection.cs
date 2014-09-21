﻿/**
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
    public enum DigestIsDefaultOptions { Regular, Digest }
    public enum MimeIsDefaultDigestOptions { Plain, Mime }
    public enum DigestVolumeFrequencyOption { Yearly, Monthly, Quarterly, Weekly, Daily }

    [Path("digest")]
    [Order(6)]
    public class DigestSection: SectionBase
    {
        public bool Digestable { get; set; }
        public DigestIsDefaultOptions DigestIsDefault { get; set; }
        public MimeIsDefaultDigestOptions MimeIsDefaultDigest { get; set; }
        public ushort DigestSizeThreshhold { get; set; }
        public bool DigestSendPeriodic { get; set; }
        public List<string> DigestHeader { get; set; }
        public List<string> DigestFooter { get; set; }
        public DigestVolumeFrequencyOption DigestVolumeFrequency { get; set; }
        
        public void NewVolume()
        {
            this.Client.ExecutePostAdminRequest(_paths.First(), "_new_volume", 1);
        }

        public void SendDigestNow()
        {
            this.Client.ExecutePostAdminRequest(_paths.First(), "_send_digest_now", 1);
        }
    
        public DigestSection(MailmanList list) : base(list) { }
    }
}
