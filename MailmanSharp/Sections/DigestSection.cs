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
            this.Client.PostAdminRequest(_paths.First(), "_new_volume", 1);
        }

        public void SendDigestNow()
        {
            this.Client.PostAdminRequest(_paths.First(), "_send_digest_now", 1);
        }
    
        public DigestSection(MailmanList list) : base(list) { }
    }
}
