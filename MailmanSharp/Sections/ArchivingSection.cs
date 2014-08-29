using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp.Sections
{
    public enum ArchivePrivateOption { Public, Private }
    public enum ArchiveVolumeFrequencyOption { Yearly, Monthly, Quarterly, Weekly, Daily }

    [Path("archive")]
    public class ArchivingSection: SectionBase
    {
        public bool Archive { get; set; }
        public ArchivePrivateOption ArchivePrivate { get; set; }
        public ArchiveVolumeFrequencyOption ArchiveVolumeFrequency { get; set; }

        public ArchivingSection(MailmanList list) : base(list) { }
    }
}
