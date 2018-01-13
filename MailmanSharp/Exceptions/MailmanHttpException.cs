using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{

    [Serializable]
    public class MailmanHttpException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public MailmanHttpException(HttpStatusCode statusCode): this(statusCode, "", null)
        {
        }

        public MailmanHttpException(HttpStatusCode statusCode, string message): this(statusCode, message, null)
        {
        }

        public MailmanHttpException(HttpStatusCode statusCode, string message, Exception inner): base(message, inner)
        {
            this.StatusCode = statusCode;
        }
    }
}
