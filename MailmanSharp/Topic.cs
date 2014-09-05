using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public class Topic
    {
        public string Name { get; set; }
        public string Regex { get; set; }
        public string Description { get; set; }

        public Topic() { }

        public Topic(string name, string regex, string description)
        {
            this.Name = name;
            this.Regex = regex;
            this.Description = description;
        }
    }
}
