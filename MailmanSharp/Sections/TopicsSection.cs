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

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    [Path("topics")]
    [Order(13)]
    public class TopicsSection: SectionBase
    {
        /// <summary>
        /// Should the topic filter be enabled or disabled?
        /// </summary>
        public bool? TopicsEnabled { get; set; }
        /// <summary>
        /// How many body lines should the topic matcher scan? 
        /// </summary>
        public ushort? TopicsBodylinesLimit { get; set; }
        /// <summary>
        /// Topics to match against each message.
        /// </summary>
        [MailmanIgnore]
        public List<Topic> Topics { get; set; } = new List<Topic>();

        internal TopicsSection(MailmanList list) : base(list) { }

        private static readonly string _nameTag = "topic_box_";
        private static readonly string _regexTag = "topic_rebox_";
        private static readonly string _descTag = "topic_desc_";

        protected override void DoAfterRead(Dictionary<string, HtmlDocument> docs)
        {
            Topics.Clear();
            var doc = docs.Single().Value;

            int i = 0;
            while (doc.DocumentNode.SafeSelectNodes(String.Format("//input[@name='topic_delete_{0:D2}']", ++i)).Any())
            {
                string index = i.ToString("D2");
                this.Topics.Add(new Topic()
                {
                    Name = (string)doc.GetInputValue(_nameTag + index),
                    Regexes = doc.GetTextAreaListValue(_regexTag + index),
                    Description = doc.GetTextAreaListValue(_descTag + index).Cat(),
                });
            }

        }

        protected override void DoBeforeFinishWrite(RestSharp.RestRequest req)
        {
            for (int i = 0; i < this.Topics.Count; ++i)
            {
                var topic = this.Topics[i];
                string index = (i + 1).ToString("D2");
                req.AddParameter(_nameTag + index, topic.Name);
                req.AddParameter(_regexTag + index, topic.Regexes.Cat());
                req.AddParameter(_descTag + index, topic.Description);
            }
        }
    }
}
