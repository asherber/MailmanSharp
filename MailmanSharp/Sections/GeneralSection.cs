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
    public enum FromIsListOption { No, MungeFrom, WrapMessage }
    public enum ReplyGoesToListOption { Poster, ThisList, ExplicitAddress }
    [Flags]
    public enum NewMemberOptions { None = 0, Hide = 1, Ack = 2, NotMeToo = 4, NoDupes = 8 }

    [Path("general")]
    [Order(1)]
    public class GeneralSection: SectionBase
    {
        public string RealName { get { return _realName; } set { SetRealName(value); } }
        public List<string> Owner { get; set; } = new List<string>();
        public List<string> Moderator { get; set; } = new List<string>();
        public string Description { get; set; } = "";
        public string Info { get; set; } = "";
        public string SubjectPrefix { get; set; } = "";
        public FromIsListOption FromIsList { get; set; }
        public bool AnonymousList { get; set; }
        public bool FirstStripReplyTo { get; set; }
        public ReplyGoesToListOption ReplyGoesToList { get; set; }
        public string ReplyToAddress { get; set; } = "";
        public bool UmbrellaList { get; set; }
        public string UmbrellaMemberSuffix { get; set; } = "";
        public bool SendReminders { get; set; }
        public string WelcomeMsg { get; set; } = "";
        public bool SendWelcomeMsg { get; set; }
        public string GoodbyeMsg { get; set; } = "";
        public bool SendGoodbyeMsg { get; set; }
        public bool AdminImmedNotify { get; set; }
        public bool AdminNotifyMchanges { get; set; }
        public bool RespondToPostRequests { get; set; }
        public bool Emergency { get; set; }
        public NewMemberOptions NewMemberOptions { get; set; }
        public bool Administrivia { get; set; }
        public ushort MaxMessageSize { get; set; }
        public ushort AdminMemberChunksize { get; set; }
        public bool IncludeRfc2369Headers { get; set; }
        public bool IncludeListPostHeader { get; set; }
        public bool IncludeSenderHeader { get; set; }
        public ushort MaxDaysToHold { get; set; }

        private string _realName = "";

        internal GeneralSection(MailmanList list) : base(list) { }

        private void SetRealName(string value)
        {
            // Only allow case changes
            if (value != _realName)
            {
                if (!String.IsNullOrWhiteSpace(_realName) && String.Compare(value, _realName, true) != 0)
                    throw new ArgumentOutOfRangeException("RealName", "RealName can only differ by case.");
                else
                    _realName = value;
            }
        }
    }
}
