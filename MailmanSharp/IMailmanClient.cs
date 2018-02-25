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

using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public interface IMailmanClient
    {
        IAuthenticator Authenticator { get; set; }
        X509CertificateCollection ClientCertificates { get; set; }
        bool FollowRedirects { get; set; }
        int? MaxRedirects { get; set; }
        IWebProxy Proxy { get; set; }
        int Timeout { get; set; }
        string UserAgent { get; set; }
        bool UseSynchronizationContext { get; set; }
        CookieContainer CookieContainer { get; set; }

        /// <summary>
        /// Reset client to default values.
        /// </summary>
        void Reset();
        /// <summary>
        /// Execute a request against the Mailman admin pages.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IRestResponse> ExecuteAdminRequestAsync(string path, IRestRequest request);
        /// <summary>
        /// Execute a request against the Mailman admin pages.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="path"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        Task<IRestResponse> ExecuteAdminRequestAsync(Method method, string path, params (string Name, object Value)[] parms);
        /// <summary>
        /// Execute a request to retrieve the list of subscribers.
        /// </summary>
        /// <returns></returns>
        Task<IRestResponse> ExecuteRosterRequestAsync();
    }

    internal interface IMailmanClientInternal: IMailmanClient
    {
        string AdminUrl { get; set; }
        string AdminPassword { get; set; }
        IMailmanClientInternal Clone();
    }
}
