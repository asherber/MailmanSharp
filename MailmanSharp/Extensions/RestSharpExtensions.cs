/**
 * Copyright 2014-5 Aaron Sherber
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

using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp.Extensions
{
    public static class RestSharpExtensions
    {
        public static IRestRequest AddOrSetParameter(this IRestRequest req, string name, object value)
        {
            var parms = req.Parameters.Where(p => String.Compare(p.Name, name, true) == 0);
            if (parms.Any())
                parms.First().Value = value;
            else
                req.AddParameter(name, value);

            return req;
        }

        public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        {
            return ((int)statusCode >= 200) && ((int)statusCode <= 299);
        }

        public static void EnsureSuccessStatusCode(this IRestResponse response)
        {
            if (!response.StatusCode.IsSuccessStatusCode())
            {
                throw new MailmanHttpException(response.StatusCode, $"{(int)response.StatusCode}: {response.StatusDescription}");
            }
        }
    }
}
