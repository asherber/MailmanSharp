using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public interface IMailmanList
    {
        string AdminUrl { get; set; }
        string AdminPassword { get; set; }
        string CurrentConfig { get; }
        string SafeCurrentConfig { get; }
        string MailmanVersion { get; }

        MembershipSection Membership { get; }
        PrivacySection Privacy { get; }
        GeneralSection General { get; }
        NonDigestSection NonDigest { get; }
        DigestSection Digest { get; }
        BounceProcessingSection BounceProcessing { get; }
        ArchivingSection Archiving { get; }
        MailNewsGatewaysSection MailNewsGateways { get; }
        ContentFilteringSection ContentFiltering { get; }
        PasswordsSection Passwords { get; }
        AutoResponderSection AutoResponder { get; }
        TopicsSection Topics { get; }

        IMailmanClient Client { get; }

        Task ReadAsync();
        Task WriteAsync();

        void LoadConfig(string xml);
        void Reset();
    }
}
