using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    [AttributeUsage(AttributeTargets.Class)]
    class OrderAttribute : Attribute
    {
        public int Value { get; set; }

        public OrderAttribute(int value)
        {
            this.Value = value;
        }
    }
}
