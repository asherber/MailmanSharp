/**
 * Copyright 2014 Aaron Sherber
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
        public List<Topic> TopicList { get; set; }

        internal TopicsSection(MailmanList list) : base(list) { }

        protected override void DoAfterRead(Dictionary<string, HtmlDocument> docs)
        {
            // read topics
            var doc = docs.Single().Value;

            int i = 0;
            while (doc.DocumentNode.SafeSelectNodes(String.Format("//input[@name='topic_delete_{0:D2}']", ++i)).Any())
            {
                this.TopicList.Add(new Topic()
                {
                    Name = GetNode(doc, "input", "box", i).Attributes["value"].Value,
                    Regex = GetNode(doc, "textarea", "rebox", i).InnerText,
                    Description = GetNode(doc, "textarea", "desc", i).InnerText,
                });
            }

        }

        protected override void DoBeforeFinishWrite(RestSharp.RestRequest req)
        {
            for (int i = 0; i < this.TopicList.Count; ++i)
            {
                var topic = this.TopicList[i];
                string index = (i + 1).ToString("D2");
                req.AddParameter("topic_box_" + index, topic.Name);
                req.AddParameter("topic_rebox_" + index, topic.Regex);
                req.AddParameter("topic_desc_" + index, topic.Description);
            }
        }

        private HtmlNode GetNode(HtmlDocument doc, string type, string tag, int index)
        {
            string xpath = String.Format("//{0}[@name='topic_{1}_{2:D2}']", type, tag, index);
            return doc.DocumentNode.SafeSelectNodes(xpath).Single();
        }

        
    }
}
