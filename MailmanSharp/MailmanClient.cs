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
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MailmanSharp
{
    public class MailmanClient
    {
        /// <summary>
        /// Url to the admin page for this list (e.g., http://foo.com/mailman/admin/mylist).
        /// </summary>
        internal string AdminUrl { get { return GetAdminUrl(); } set { SetAdminUrl(value); } }
        /// <summary>
        /// Administrator password for list.
        /// </summary>
        internal string AdminPassword { get; set; }

        private string _listName;
        private string _adminPath;
        private MailmanList _list;
        private RestClient _client;

        internal MailmanClient(MailmanList list)
        {
            _list = list ?? throw new ArgumentNullException("list");

            _client = new RestClient();
            _client.FollowRedirects = true;
            _client.CookieContainer = new System.Net.CookieContainer();            
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
            };

            result._client.Authenticator = _client.Authenticator;
            //result._client.BaseUrl = _client.BaseUrl;
            result._client.ClientCertificates = _client.ClientCertificates;
            result._client.FollowRedirects = _client.FollowRedirects;
            result._client.MaxRedirects = _client.MaxRedirects;
            result._client.Proxy = _client.Proxy;
            result._client.Timeout = _client.Timeout;
            result._client.UserAgent = _client.UserAgent;
            result._client.UseSynchronizationContext = _client.UseSynchronizationContext;
            

            foreach (var cookie in _client.CookieContainer.GetCookies(_client.BaseUrl))
                result._client.CookieContainer.Add((Cookie)cookie);
            
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
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                _list.MailmanVersion = Regex.Match(resp.Content, @"(?<=Delivered by Mailman.*version ).*(?=<)").Value;
                return resp;
            }
            else
            {
                string msg = String.Format("Request failed. {{Uri={0}, Message={1}}}", resp.ResponseUri, resp.StatusDescription);
                throw new Exception(msg);
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
