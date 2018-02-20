using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{

    [Serializable]
    public class MailmanException : Exception
    {
        public MailmanException() { }
        public MailmanException(string message) : base(message) { }
        public MailmanException(string message, Exception inner) : base(message, inner) { }
        protected MailmanException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
