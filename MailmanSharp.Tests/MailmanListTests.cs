using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using RestSharp;
using Newtonsoft.Json.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalTests.Namers;
using RestSharp.Authenticators;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using FluentAssertions.Json;

namespace MailmanSharp.Tests
{
    [UseApprovalSubdirectory("Approvals")]
    [UseReporter(typeof(DiffReporter))]
    public class MailmanListTests
    {
        private readonly Mock<IMailmanClientInternal> _clientMock;
        private readonly IMailmanList _list;
        
        private static string _adminUrl = "http://example.com/admin.cgi/foo-list";
        private static string _password = "password";

        public MailmanListTests()
        {
            _list = new MailmanList();

            _clientMock = new Mock<IMailmanClientInternal>();
            _clientMock.SetupProperty(c => c.AdminUrl);
            _clientMock.SetupProperty(c => c.AdminPassword);
            var prop = typeof(MailmanList).GetProperty("InternalClient", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(_list, _clientMock.Object);
        }

        [Fact]
        public void New_List_Initializes_Sections()
        {
            _list.Membership.Should().NotBeNull();
            _list.Privacy.Should().NotBeNull();
            _list.General.Should().NotBeNull();
            _list.NonDigest.Should().NotBeNull();
            _list.Digest.Should().NotBeNull();
            _list.BounceProcessing.Should().NotBeNull();
            _list.Archiving.Should().NotBeNull();
            _list.MailNewsGateways.Should().NotBeNull();
            _list.ContentFiltering.Should().NotBeNull();
            _list.Passwords.Should().NotBeNull();
            _list.AutoResponder.Should().NotBeNull();
            _list.Topics.Should().NotBeNull();
        }

        [Fact]
        public void New_List_Has_Default_Config()
        {
            var output = JObject.Parse(_list.CurrentConfig);
            output.Remove("Meta");
            Approvals.Verify(output.ToString());
        }

        [Fact]
        public void Setting_AdminUrl_Hits_Client()
        {
            _list.AdminUrl = _adminUrl;

            _clientMock.VerifySet(c => c.AdminUrl = _adminUrl);
        }

        [Fact]
        public void Setting_Password_Hits_Client()
        {
            _list.AdminPassword = _password;

            _clientMock.VerifySet(c => c.AdminPassword = _password);
        }

        [Fact]
        public void LoadConfig_Works()
        {
            _list.LoadConfig(File.ReadAllText("ChangedConfig.json"));
            var output = JObject.Parse(_list.CurrentConfig);
            output.Remove("Meta");

            Approvals.Verify(output.ToString());
        }

        [Fact]
        public async Task ReadAsync_Logs_In_And_Does_All_Reads()
        {
            _clientMock.Setup(c => c.ExecuteAdminRequestAsync(Method.GET, null))
                .ReturnsAsync(new RestResponse())
                .Verifiable();
            _clientMock.Setup(c => c.ExecuteAdminRequestAsync(Method.GET, It.IsNotNull<string>()))
                .ReturnsAsync(new RestResponse()
                {
                    Content = File.ReadAllText("general.html"),
                    StatusCode = System.Net.HttpStatusCode.OK
                });
            _clientMock.Setup(c => c.Clone())
                .Returns(_clientMock.Object);
            _clientMock.Setup(c => c.ExecuteRosterRequestAsync())
                .ReturnsAsync(new RestResponse()
                {
                    Content = "<html><head></head><body></body></html>",
                    StatusCode = System.Net.HttpStatusCode.OK
                });

            await _list.ReadAsync();

            _clientMock.Verify();
            // There should be 15 reads, but Membership and Passwords don't read initially
            _clientMock.Verify(c => c.ExecuteAdminRequestAsync(Method.GET, It.IsNotNull<string>()), Times.Exactly(13));
        }

        [Fact]
        public async Task WriteAsync_Logs_In_And_Does_All_Reads()
        {
            _clientMock.Setup(c => c.ExecuteAdminRequestAsync(Method.GET, null))
                .ReturnsAsync(new RestResponse())
                .Verifiable();
            _clientMock.Setup(c => c.Clone())
                .Returns(_clientMock.Object);

            await _list.WriteAsync();

            _clientMock.Verify();
            // Membership doesn't post
            _clientMock.Verify(c => c.ExecuteAdminRequestAsync(It.IsAny<string>(), It.IsAny<IRestRequest>()), 
                Times.Exactly(14));
        }

        [Fact]
        public void ResetClient_Should_Work()
        {
            var list = new MailmanList();
            list.Client.Authenticator = new SimpleAuthenticator(null, null, null, null);
            list.Client.ClientCertificates = new X509CertificateCollection(new[] { new X509Certificate() });
            list.Client.FollowRedirects = false;
            list.Client.MaxRedirects = 99;
            list.Client.Proxy = new WebProxy();
            list.Client.Timeout = 42;
            list.Client.UserAgent = Guid.NewGuid().ToString();
            list.Client.UseSynchronizationContext = true;

            list.ResetClient();

            var expected = new MailmanList().Client;

            list.Client.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Reset_Should_Work()
        {
            var guid = Guid.NewGuid().ToString();
            _list.Archiving.Archive = true;
            _list.AutoResponder.AutoresponsePostingsText = guid;
            _list.BounceProcessing.BounceNotifyOwnerOnBounceIncrement = true;
            _list.ContentFiltering.FilterMimeTypes.Add(guid);
            _list.Digest.DigestFooter = guid;            
            _list.General.Description = Guid.NewGuid().ToString();
            _list.MailNewsGateways.LinkedNewsgroup = guid;
            _list.NonDigest.MsgFooter = guid;
            _list.Privacy.AcceptTheseNonmembers.Add(guid);
            _list.Topics.TopicsBodylinesLimit = 42;
            _list.AdminPassword = guid;
            _list.AdminUrl = guid;
            _list.Reset();
            

            var expected = new MailmanList();
            _list.AdminPassword.Should().Be(expected.AdminPassword);
            _list.AdminUrl.Should().Be(expected.AdminUrl);

            var config = JObject.Parse(_list.CurrentConfig);
            config["Meta"]["ExportedDate"] = DateTime.Today;
            var expectedConfig = JObject.Parse(expected.CurrentConfig);
            expectedConfig["Meta"]["ExportedDate"] = DateTime.Today;
            config.Should().BeEquivalentTo(expectedConfig);
        }
    }
}
