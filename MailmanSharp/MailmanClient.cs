using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MailmanSharp
{
    public class MailmanClient: RestClient
    {
        public string ListName { get; set; }
        public string ServerUrl { get; set; }
        public string Password { get; set; }

        public MailmanClient()
        {
            this.FollowRedirects = true;
            this.CookieContainer = new System.Net.CookieContainer();
        }

        public IRestResponse ExecuteAdminRequest(string path, RestRequest request)
        {
            if (!String.IsNullOrEmpty(path) && path.StartsWith("/"))
                path = path.Substring(1);
            var resource = String.Format("admin.cgi/{0}/{1}", ListName, path);
            var req = request ?? new RestRequest();
            req.Resource = resource;
            req.Method = Method.POST;
            req.AddParameter("adminpw", this.Password);

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
            var resource = String.Format("roster.cgi/{0}", ListName);
            var req = new RestRequest(resource, Method.POST);
            return this.Execute(req);
        }
    }
}
