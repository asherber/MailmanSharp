using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public class MailmanContractResolver: DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            
            if (typeof(SectionBase).IsAssignableFrom(prop.DeclaringType))
            {
                prop.Ignored = prop.AttributeProvider.GetAttributes(typeof(IgnoreAttribute), false).Any();
            }

            return prop;
        }
    }
}
