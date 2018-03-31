/**
 * Copyright 2014-2018 Aaron Sherber
 * 
 * This file is part of MailmanSharp.
 *
 * MailmanSharp is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * MailmanSharp is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with MailmanSharp. If not, see <http://www.gnu.org/licenses/>.
 */

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
