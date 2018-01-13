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

        void Reset();
        Task<IRestResponse> ExecuteGetAdminRequestAsync(string path, IRestRequest request);
        Task<IRestResponse> ExecuteGetAdminRequestAsync(string path, params object[] parms);
        Task<IRestResponse> ExecutePostAdminRequestAsync(string path, IRestRequest request);
        Task<IRestResponse> ExecutePostAdminRequestAsync(string path, params object[] parms);
        Task<IRestResponse> ExecuteRosterRequestAsync();
    }
}
