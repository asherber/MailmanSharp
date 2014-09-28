/**
 * Copyright 2014 Aaron Sherber
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

namespace MailmanSharp
{
    public class MailmanClient: RestClient
    {
        /// <summary>
        /// Url to the admin page for this list (e.g., http://foo.com/mailman/admin/mylist).
        /// </summary>
        internal string AdminUrl { get { return GetAdminUrl(); } set { SetAdminUrl(value); } }
        /// <summary>
        /// Administrator password for list.
        /// </summary>
        internal string AdminPassword { get; set; }

        private string ListName { get; set; }
        private string AdminPath { get; set; }
        private string RosterPath { get { return AdminPath.Replace("admin", "roster"); } }

        internal MailmanClient()
        {
            this.FollowRedirects = true;
            this.CookieContainer = new System.Net.CookieContainer();            
        }

        /// <summary>
        /// Create a copy of a MailmanClient.
        /// </summary>
        /// <returns>New MailmanClient</returns>
        internal MailmanClient Clone()
        {
            var result = new MailmanClient()
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

            foreach (var cookie in this.CookieContainer.GetCookies(new Uri(BaseUrl)))
                result.CookieContainer.Add((Cookie)cookie);
            
            foreach (var param in this.DefaultParameters)
                result.DefaultParameters.Add(param);

            return result;
        }

        public IRestResponse ExecuteGetAdminRequest(string path, IRestRequest request)
        {
            return DoAdminRequest(path, request, Method.GET);
        }

        public IRestResponse ExecuteGetAdminRequest(string path, params object[] parms)
        {
            var req = BuildRequestFromParms(parms);
            return this.ExecuteGetAdminRequest(path, req);
        }

        public IRestResponse ExecutePostAdminRequest(string path, IRestRequest request)
        {
            return DoAdminRequest(path, request, Method.POST);
        }

        public IRestResponse ExecutePostAdminRequest(string path, params object[] parms)
        {
            var req = BuildRequestFromParms(parms);
            return this.ExecutePostAdminRequest(path, req);
        }

        private IRestResponse DoAdminRequest(string path, IRestRequest request, Method method)
        {
            if (!String.IsNullOrEmpty(path))
                path = path.Trim('/');

            var req = request ?? new RestRequest();
            req.Resource = String.Format("{0}/{1}/{2}", AdminPath, ListName, path); ;
            req.Method = method;
            req.AddOrSetParameter("adminpw", this.AdminPassword);

            var resp = this.Execute(req);
            if (resp.StatusCode == HttpStatusCode.OK)
                return resp;
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

        public IRestResponse ExecuteRosterRequest()
        {
            if (!HasAdminCookie())
                ExecuteGetAdminRequest("");
            var resource = String.Format("{0}/{1}", RosterPath, ListName);
            var req = new RestRequest(resource);
            req.AddParameter("adminpw", this.AdminPassword);
            return this.Execute(req);
        }

        internal bool HasAdminCookie()
        {
            var cookies = CookieContainer.GetCookies(new Uri(BaseUrl));
            return cookies.Cast<Cookie>().Any(c => c.Name == String.Format("{0}+admin", ListName));
        }
        
        private string GetAdminUrl()
        {
            // We know these bits have no trailing slashes.
            // BaseUrl gets trimmed by RestClient; the other two get trimmed in SetAdminUrl.
            var url = String.Format("{0}/{1}/{2}", BaseUrl, AdminPath, ListName);
            return Regex.IsMatch(url, "\\w") ? url : "";
        }

        private void SetAdminUrl(string value)
        {
            BaseUrl = "";
            AdminPath = "";
            ListName = "";

            if (!String.IsNullOrWhiteSpace(value))
            {
                var uri = new Uri(value);
                int numSegs = uri.Segments.Length;

                if (numSegs < 3)
                    throw new ArgumentException("AdminUrl must have at least 3 path segments.");

                BaseUrl = uri.GetLeftPart(UriPartial.Authority)
                    + String.Join("", uri.Segments.Take(numSegs - 2));
                AdminPath = uri.Segments[numSegs - 2].TrimEnd('/');
                ListName = uri.Segments.Last().TrimEnd('/');
            }
        }
    }
}
