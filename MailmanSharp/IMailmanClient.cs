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
        Task<IRestResponse> ExecuteAdminRequestAsync(string path, IRestRequest request);
        Task<IRestResponse> ExecuteAdminRequestAsync(Method method, string path, params (string Name, object Value)[] parms);
        Task<IRestResponse> ExecuteRosterRequestAsync();
    }

    internal interface IMailmanClientInternal: IMailmanClient
    {
        string AdminUrl { get; set; }
        string AdminPassword { get; set; }
        IMailmanClientInternal Clone();
    }
}
