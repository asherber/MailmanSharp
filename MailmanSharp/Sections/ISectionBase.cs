using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public interface ISectionBase
    {
        Task ReadAsync();
        Task WriteAsync();
    }
}
