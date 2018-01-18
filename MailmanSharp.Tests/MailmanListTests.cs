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
            _clientMock.Setup(c => c.ExecuteGetAdminRequestAsync(""))
                .ReturnsAsync(new RestResponse())
                .Verifiable();
            _clientMock.Setup(c => c.ExecuteGetAdminRequestAsync(It.IsNotIn("")))
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
            _clientMock.Verify(c => c.ExecuteGetAdminRequestAsync(It.IsNotIn("")), Times.Exactly(13));
        }

        [Fact]
        public async Task WriteAsync_Logs_In_And_Does_All_Reads()
        {
            _clientMock.Setup(c => c.ExecuteGetAdminRequestAsync(""))
                .ReturnsAsync(new RestResponse())
                .Verifiable();
            _clientMock.Setup(c => c.Clone())
                .Returns(_clientMock.Object);

            await _list.WriteAsync();

            _clientMock.Verify();
            // Membership doesn't post
            _clientMock.Verify(c => c.ExecutePostAdminRequestAsync(It.IsAny<string>(), It.IsAny<IRestRequest>()), 
                Times.Exactly(14));
        }
    }
}
