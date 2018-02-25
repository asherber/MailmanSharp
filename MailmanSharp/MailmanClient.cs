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
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public class MailmanClient: IMailmanClientInternal
    {
        string IMailmanClientInternal.AdminUrl { get { return GetAdminUrl(); } set { SetAdminUrl(value); } }
        private string AdminUrl { get => ((IMailmanClientInternal)this).AdminUrl; set => ((IMailmanClientInternal)this).AdminUrl = value; }
        string IMailmanClientInternal.AdminPassword { get; set; }
        private string AdminPassword { get => ((IMailmanClientInternal)this).AdminPassword; set => ((IMailmanClientInternal)this).AdminPassword = value; }

        public IAuthenticator Authenticator { get => _client.Authenticator; set => _client.Authenticator = value; }
        public X509CertificateCollection ClientCertificates { get => _client.ClientCertificates; set => _client.ClientCertificates = value; }
        public bool FollowRedirects { get => _client.FollowRedirects; set => _client.FollowRedirects = value; }
        public int? MaxRedirects { get => _client.MaxRedirects; set => _client.MaxRedirects = value; }
        public IWebProxy Proxy { get => _client.Proxy; set => _client.Proxy = value; }
        public int Timeout { get => _client.Timeout; set => _client.Timeout = value; }
        public string UserAgent { get => _client.UserAgent; set => _client.UserAgent = value; }
        public bool UseSynchronizationContext { get => _client.UseSynchronizationContext; set => _client.UseSynchronizationContext = value; }
        

        private string _listName;
        private string _adminPath;
        private MailmanList _list;
        private IRestClient _client;

        internal MailmanClient(MailmanList list)
        {
            _list = list ?? throw new ArgumentNullException("list");

            _client = new RestClient
            {
                FollowRedirects = true,
                CookieContainer = new System.Net.CookieContainer()
            };
        }

        /// <summary>
        /// Create a copy of a MailmanClient.
        /// </summary>
        /// <returns>New MailmanClient</returns>
        IMailmanClientInternal IMailmanClientInternal.Clone()
        {
            var result = new MailmanClient(_list)
            {
                AdminUrl = this.AdminUrl,
                AdminPassword = this.AdminPassword,

                Authenticator = this.Authenticator,
                //BaseUrl = this.BaseUrl,
                ClientCertificates = this.ClientCertificates,
                FollowRedirects = this.FollowRedirects,
                MaxRedirects = this.MaxRedirects,
                Proxy = this.Proxy,
                Timeout = this.Timeout,
                UserAgent = this.UserAgent,
                UseSynchronizationContext = this.UseSynchronizationContext,
            };

            foreach (var cookie in _client.CookieContainer.GetCookies(_client.BaseUrl))
                result._client.CookieContainer.Add((Cookie)cookie);
            
            foreach (var param in _client.DefaultParameters)
                result._client.DefaultParameters.Add(param);

            return result;
        }

        /// <summary>
        /// Reset client to default values.
        /// </summary>
        public void Reset()
        {
            _list.ResetClient();
        }

        private static Regex _versionRe = new Regex(@"(?<=Delivered by Mailman.*version ).*(?=<)");

        /// <summary>
        /// Execute a request against the Mailman admin pages.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IRestResponse> ExecuteAdminRequestAsync(string path, IRestRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!String.IsNullOrEmpty(path))
                path = path.Trim('/');

            request.Resource = String.Format("{0}/{1}/{2}", _adminPath, _listName, path);
            request.AddOrSetParameter("adminpw", this.AdminPassword);

            var resp = await _client.ExecuteTaskAsync(request).ConfigureAwait(false);

            resp.CheckResponseAndThrowIfNeeded();

            _list.SetMailmanVersion(_versionRe.Match(resp.Content).Value);
            return resp;            
        }

        /// <summary>
        /// Execute a request against the Mailman admin pages.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="path"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public Task<IRestResponse> ExecuteAdminRequestAsync(Method method, string path, params (string Name, object Value)[] parms)
        {
            var req = new RestRequest(method);
            foreach (var parm in parms)
            {
                req.AddParameter(parm.Name, parm.Value);
            }
            
            return this.ExecuteAdminRequestAsync(path, req);
        }

        private string GetRosterPath() 
        { 
            return _adminPath.Replace("admin", "roster"); 
        }

        /// <summary>
        /// Execute a request to retrieve the list of subscribers.
        /// </summary>
        /// <returns></returns>
        public async Task<IRestResponse> ExecuteRosterRequestAsync()
        {
            if (!HasAdminCookie())
                await ExecuteAdminRequestAsync(Method.GET, null).ConfigureAwait(false);
            var resource = String.Format("{0}/{1}", this.GetRosterPath(), _listName);
            var req = new RestRequest(resource);
            req.AddParameter("adminpw", this.AdminPassword);
            var resp = await _client.ExecuteTaskAsync(req).ConfigureAwait(false);

            resp.CheckResponseAndThrowIfNeeded();

            return resp;
        }

        internal bool HasAdminCookie()
        {
            var cookies = _client.CookieContainer.GetCookies(_client.BaseUrl);
            return cookies.Cast<Cookie>().Any(c => c.Name == String.Format("{0}+admin", _listName));
        }
        
        private string GetAdminUrl()
        {
            // We know these bits have no trailing slashes.
            // BaseUrl gets trimmed by RestClient; the other two get trimmed in SetAdminUrl.
            var url = String.Format("{0}/{1}/{2}", _client.BaseUrl, _adminPath, _listName);
            return Regex.IsMatch(url, "\\w") ? url : "";
        }

        private void SetAdminUrl(string value)
        {
            _client.BaseUrl = null;
            _adminPath = "";
            _listName = "";

            if (!String.IsNullOrWhiteSpace(value))
            {
                var uri = new Uri(value);
                int numSegs = uri.Segments.Length;

                if (numSegs < 3)
                    throw new ArgumentException("AdminUrl must have at least 3 path segments.");

                _client.BaseUrl = new Uri(uri.GetLeftPart(UriPartial.Authority)
                    + String.Join("", uri.Segments.Take(numSegs - 2)));
                _adminPath = uri.Segments[numSegs - 2].TrimEnd('/');
                _listName = uri.Segments.Last().TrimEnd('/');
            }
        }
    }
}
