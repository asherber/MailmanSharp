using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MailmanSharp
{
    public class MailmanClient: RestClient
    {
        /// <summary>
        /// Should end in admin or admin.cgi; do not include list name.
        /// </summary>        
        public string BaseAdminUrl { get { return GetBaseAdminUrl(); } set { SetBaseAdminUrl(value); } }
        public string ListName { get; set; }
        public string AdminPassword { get; set; }

        private string AdminSeg { get; set; }
        private string RosterSeg { get { return AdminSeg.Replace("admin", "roster"); } }

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
                AdminPassword = this.AdminPassword,
                BaseAdminUrl = this.BaseAdminUrl,

                Authenticator = this.Authenticator,
                //BaseUrl = this.BaseUrl,
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
            if (!String.IsNullOrEmpty(path))
                path = path.Trim('/');

            var req = request ?? new RestRequest();
            req.Resource = String.Format("{0}/{1}/{2}", AdminSeg, ListName, path);;
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
            var resource = String.Format("{0}/{1}", RosterSeg, ListName);
            var req = new RestRequest(resource, Method.POST);
            req.AddParameter("adminpw", this.AdminPassword);
            return this.Execute(req);
        }

        public void Login()
        {
            ExecuteAdminRequest("");
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
                req.AddParameter("adminpw", this.AdminPassword);
            else
                parm.Value = this.AdminPassword;
        }
        private void SetBaseAdminUrl(string value)
        {
            var uri = new Uri(value);
            var sb = new StringBuilder();
            sb.AppendFormat("{0}://{1}", uri.Scheme, uri.Host);
            foreach (var seg in uri.Segments.Take(uri.Segments.Length - 1))
                sb.Append(seg);
            this.BaseUrl = sb.ToString();

            AdminSeg = uri.Segments.Last().TrimEnd('/');
        }

        private string GetBaseAdminUrl()
        {
            var ub = new UriBuilder(this.BaseUrl) { Path = AdminSeg };
            return ub.ToString();
        }        
    }
}
