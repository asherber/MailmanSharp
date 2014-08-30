﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MailmanSharp
{
    public class MailmanClient: RestClient
    {
        public string ListName { get; set; }
        public string Password { get; set; }

        public MailmanClient()
        {
            this.FollowRedirects = true;
            this.CookieContainer = new System.Net.CookieContainer();            
        }

        public MailmanClient Clone()
        {
            var result = new MailmanClient()
            {
                ListName = this.ListName,
                Password = this.Password,

                Authenticator = this.Authenticator,
                BaseUrl = this.BaseUrl,
                ClientCertificates = this.ClientCertificates,
                //CookieContainer
                FollowRedirects = this.FollowRedirects,
                MaxRedirects = this.MaxRedirects,
                Proxy = this.Proxy,
                Timeout = this.Timeout,
                UserAgent = this.UserAgent,
                UseSynchronizationContext = this.UseSynchronizationContext,
            };

            foreach (var param in this.DefaultParameters)
                result.DefaultParameters.Add(param);

            return result;
        }

        public IRestResponse ExecuteAdminRequest(string path, RestRequest request)
        {
            if (!String.IsNullOrEmpty(path) && path.StartsWith("/"))
                path = path.Substring(1);
            var resource = String.Format("admin.cgi/{0}/{1}", ListName, path);
            var req = request ?? new RestRequest();
            req.Resource = resource;
            req.Method = Method.POST;
            EnsureAdminPassword(req);
            
            return this.Execute(req);
        }

        public IRestResponse ExecuteAdminRequest(string path, IEnumerable<Parameter> parms)
        {
            var req = new RestRequest();
            req.Parameters.AddRange(parms);
            return this.ExecuteAdminRequest(path, req);
        }

        public IRestResponse ExecuteAdminRequest(string path)
        {
            return ExecuteAdminRequest(path, (RestRequest)null);
        }

        public IRestResponse ExecuteAdminRequest(string path, params object[] parms)
        {
            if (parms.Length % 2 != 0)
                throw new ArgumentException("Argument 'parms' must have even number of values");

            var req = new RestRequest();
            for (int i = 0; i < parms.Length; i += 2)
            {
                req.AddParameter(parms[i].ToString(), parms[i + 1]);
            }

            return this.ExecuteAdminRequest(path, req);
        }

        

        public IRestResponse ExecuteRosterRequest()
        {
            if (!HasAdminCookie()) Login();
            var resource = String.Format("roster.cgi/{0}", ListName);
            var req = new RestRequest(resource, Method.POST);
            req.AddParameter("adminpw", this.Password);
            return this.Execute(req);
        }

        private bool HasAdminCookie()
        {
            var cookies = CookieContainer.GetCookies(new Uri(BaseUrl));
            return cookies.Cast<Cookie>().Any(c => c.Name == String.Format("{0}+admin", ListName));
        }

        private void EnsureAdminPassword(RestRequest req)
        {
            var parm = req.Parameters.FirstOrDefault(p => p.Name == "adminpw");
            if (parm == null)
                req.AddParameter("adminpw", this.Password);
            else
                parm.Value = this.Password;
        }

        public void Login()
        {
            ExecuteAdminRequest("");
        }
    }
}
