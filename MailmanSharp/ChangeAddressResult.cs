using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    [DebuggerDisplay("{Message}")]
    public class ChangeAddressResult
    {
        public bool Success => Message?.Contains("changed to") == true;
        public string Message { get; set; }

        public ChangeAddressResult(string message)
        {
            Message = message;
        }
    }
}
