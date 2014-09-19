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
        public bool Archive { get; set; }
        public ArchivePrivateOption ArchivePrivate { get; set; }
        public ArchiveVolumeFrequencyOption ArchiveVolumeFrequency { get; set; }

        public ArchivingSection(MailmanList list) : base(list) { }
    }
}
