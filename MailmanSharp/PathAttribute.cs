using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    class PathAttribute : Attribute
    {
        public string Value { get; set; }

        public PathAttribute(string value)
        {
            this.Value = value;
        }
    }
}
