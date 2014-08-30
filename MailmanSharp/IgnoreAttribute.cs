using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    [AttributeUsage(AttributeTargets.Property)]
    class IgnoreAttribute : Attribute
    {       
    }
}
