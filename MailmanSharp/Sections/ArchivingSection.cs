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
    public enum ArchivePrivateOption { Public, Private }
    public enum ArchiveVolumeFrequencyOption { Yearly, Monthly, Quarterly, Weekly, Daily }

    [Path("archive")]
    [Order(9)]
    public class ArchivingSection: SectionBase
    {
        /// <summary>
        /// Archive messages?
        /// </summary>
        public bool? Archive { get; set; }
        /// <summary>
        /// Is archive file source for public or private archival?
        /// </summary>
        public ArchivePrivateOption? ArchivePrivate { get; set; }
        /// <summary>
        /// How often should a new archive volume be started?
        /// </summary>
        public ArchiveVolumeFrequencyOption? ArchiveVolumeFrequency { get; set; }

        internal ArchivingSection(MailmanList list) : base(list) { }
    }
}
