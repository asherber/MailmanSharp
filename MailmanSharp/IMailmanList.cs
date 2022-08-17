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
using System.Threading.Tasks;

namespace MailmanSharp
{
    public interface IMailmanList
    {
        /// <summary>
        /// Url to the admin page for this list (e.g., http://foo.com/mailman/admin/mylist).
        /// </summary>
        string AdminUrl { get; set; }
        /// <summary>
        /// Administrator password for list.
        /// </summary>
        string AdminPassword { get; set; }
        /// <summary>
        /// Current configuration of object as JSON.
        /// </summary>
        string CurrentConfig { get; }
        /// <summary>
        /// Gets version of Mailman that this list is running on.
        /// </summary>
        MailmanVersion MailmanVersion { get; }

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

        /// <summary>
        /// Client which communicates with Mailman.
        /// </summary>
        IMailmanClient Client { get; }

        /// <summary>
        /// Read properties for all sections from Mailman.
        /// </summary>
        /// <returns></returns>
        Task ReadAsync();
        /// <summary>
        /// Write properties for all sections to Mailman.
        /// </summary>
        /// <returns></returns>
        Task WriteAsync();

        /// <summary>
        /// Load configuration for this list from JSON string.
        /// </summary>
        /// <param name="json"></param>
        void LoadConfig(string json);
        /// <summary>
        /// Reset all properties to defaults.
        /// </summary>
        void Reset();
    }
}
