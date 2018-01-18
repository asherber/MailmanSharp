using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public class MailmanJsonSerializer: JsonSerializer
    {
        public MailmanJsonSerializer()
        {
            ContractResolver = new MailmanContractResolver();
            Converters.Add(new StringEnumConverter());
        }
    }
}
