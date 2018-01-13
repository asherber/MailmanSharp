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
        public bool TopicsEnabled { get; set; }
        public ushort TopicsBodylinesLimit { get; set; }
        [Ignore]
        public List<Topic> TopicList { get; set; } = new List<Topic>();

        internal TopicsSection(MailmanList list) : base(list) { }

        private static readonly string _nameTag = "topic_box_";
        private static readonly string _regexTag = "topic_rebox_";
        private static readonly string _descTag = "topic_desc_";

        protected override void DoAfterRead(Dictionary<string, HtmlDocument> docs)
        {
            // read topics
            var doc = docs.Single().Value;

            int i = 0;
            while (doc.DocumentNode.SafeSelectNodes(String.Format("//input[@name='topic_delete_{0:D2}']", ++i)).Any())
            {
                string index = i.ToString("D2");
                this.TopicList.Add(new Topic()
                {
                    Name = (string)doc.GetNodeValue(_nameTag + index),
                    Regexes = doc.GetNodeListValue(_regexTag + index),
                    Description = doc.GetNodeListValue(_descTag + index).Cat(),
                });
            }

        }

        protected override void DoBeforeFinishWrite(RestSharp.RestRequest req)
        {
            for (int i = 0; i < this.TopicList.Count; ++i)
            {
                var topic = this.TopicList[i];
                string index = (i + 1).ToString("D2");
                req.AddParameter(_nameTag + index, topic.Name);
                req.AddParameter(_regexTag + index, topic.Regexes.Cat());
                req.AddParameter(_descTag + index, topic.Description);
            }
        }
    }
}
