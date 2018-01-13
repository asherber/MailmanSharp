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

using MailmanSharp.Extensions;
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
    public class MailmanClient: IMailmanClient
    {
        /// <summary>
        /// Url to the admin page for this list (e.g., http://foo.com/mailman/admin/mylist).
        /// </summary>
        internal string AdminUrl { get { return GetAdminUrl(); } set { SetAdminUrl(value); } }
        /// <summary>
        /// Administrator password for list.
        /// </summary>
        internal string AdminPassword { get; set; }

        public IAuthenticator Authenticator { get => _client.Authenticator; set => _client.Authenticator = value; }
        public X509CertificateCollection ClientCertificates { get => _client.ClientCertificates; set => _client.ClientCertificates = value; }
        public bool FollowRedirects { get => _client.FollowRedirects; set => _client.FollowRedirects = value; }
        public int? MaxRedirects { get => _client.MaxRedirects; set => _client.MaxRedirects = value; }
        public IWebProxy Proxy { get => _client.Proxy; set => _client.Proxy = value; }
        public int Timeout { get => _client.Timeout; set => _client.Timeout = value; }
        public string UserAgent { get => _client.UserAgent; set => _client.UserAgent = value; }
        public bool UseSynchronizationContext { get => _client.UseSynchronizationContext; set => _client.UseSynchronizationContext = value; }
        public CookieContainer CookieContainer { get => _client.CookieContainer; set => _client.CookieContainer = value; }


        private string _listName;
        private string _adminPath;
        private MailmanList _list;
        private RestClient _client;

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
        internal MailmanClient Clone()
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

            foreach (var cookie in this.CookieContainer.GetCookies(_client.BaseUrl))
                result.CookieContainer.Add((Cookie)cookie);
            
            foreach (var param in _client.DefaultParameters)
                result._client.DefaultParameters.Add(param);

            return result;
        }

        public void Reset()
        {
            _list.ResetClient();
        }

        public Task<IRestResponse> ExecuteGetAdminRequestAsync(string path, IRestRequest request)
        {
            return DoAdminRequestAsync(path, request, Method.GET);
        }

        public Task<IRestResponse> ExecuteGetAdminRequestAsync(string path, params object[] parms)
        {
            var req = BuildRequestFromParms(parms);
            return this.ExecuteGetAdminRequestAsync(path, req);
        }

        public Task<IRestResponse> ExecutePostAdminRequestAsync(string path, IRestRequest request)
        {
            return DoAdminRequestAsync(path, request, Method.POST);
        }

        public Task<IRestResponse> ExecutePostAdminRequestAsync(string path, params object[] parms)
        {
            var req = BuildRequestFromParms(parms);
            return this.ExecutePostAdminRequestAsync(path, req);
        }

        private async Task<IRestResponse> DoAdminRequestAsync(string path, IRestRequest request, Method method)
        {
            if (!String.IsNullOrEmpty(path))
                path = path.Trim('/');

            var req = request ?? new RestRequest();
            req.Resource = String.Format("{0}/{1}/{2}", _adminPath, _listName, path); ;
            req.Method = method;
            req.AddOrSetParameter("adminpw", this.AdminPassword);

            var resp = await _client.ExecuteTaskAsync(req).ConfigureAwait(false);

            resp.EnsureSuccessStatusCode();
            if (resp.ErrorException != null)
                throw resp.ErrorException;
            else 
            {
                _list.MailmanVersion = Regex.Match(resp.Content, @"(?<=Delivered by Mailman.*version ).*(?=<)").Value;
                return resp;
            }     
        }

        private static IRestRequest BuildRequestFromParms(params object[] parms)
        {
            if (parms.Length % 2 != 0)
                throw new ArgumentException("Argument 'parms' must have even number of values");

            var result = new RestRequest();
            for (int i = 0; i < parms.Length; i += 2)
            {
                result.AddParameter(parms[i].ToString(), parms[i + 1]);
            }
            return result;
        }

        private string GetRosterPath() 
        { 
            return _adminPath.Replace("admin", "roster"); 
        }
        

        public async Task<IRestResponse> ExecuteRosterRequestAsync()
        {
            if (!HasAdminCookie())
                await ExecuteGetAdminRequestAsync("").ConfigureAwait(false);
            var resource = String.Format("{0}/{1}", this.GetRosterPath(), _listName);
            var req = new RestRequest(resource);
            req.AddParameter("adminpw", this.AdminPassword);
            return await _client.ExecuteTaskAsync(req).ConfigureAwait(false);
        }

        internal bool HasAdminCookie()
        {
            var cookies = CookieContainer.GetCookies(_client.BaseUrl);
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
