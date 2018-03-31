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

using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public enum NewsModerationOption { None, OpenListModeratedGroup, Moderated }

    [Path("gateway")]
    [Order(10)]
    public class MailNewsGatewaysSection: SectionBase
    {
        /// <summary>
        /// The hostname of the machine your news server is running on. 
        /// </summary>
        public string NntpHost { get; set; }
        /// <summary>
        /// The name of the Usenet group to gateway to and/or from.
        /// </summary>
        public string LinkedNewsgroup { get; set; }
        /// <summary>
        /// Should new posts to the mailing list be sent to the newsgroup? 
        /// </summary>
        public bool? GatewayToNews { get; set; }
        /// <summary>
        /// Should new posts to the newsgroup be sent to the mailing list?
        /// </summary>
        public bool? GatewayToMail { get; set; }
        /// <summary>
        /// The moderation policy of the newsgroup. 
        /// </summary>
        public NewsModerationOption? NewsModeration { get; set; }
        /// <summary>
        /// Prefix Subject: headers on postings gated to news?
        /// </summary>
        public bool? NewsPrefixSubjectToo { get; set; }
        
        /// <summary>
        /// Perform a catchup on the newsgroup.
        /// </summary>
        /// <returns></returns>
        public Task MassCatchupAsync()
        {
            return this.GetClient().ExecuteAdminRequestAsync(Method.POST, _paths.First(), ("_mass_catchup", 1));
        }

        internal MailNewsGatewaysSection(MailmanList list) : base(list) { }
    }
}
