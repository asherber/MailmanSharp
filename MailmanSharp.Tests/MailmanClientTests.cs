using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using RestSharp;
using System.Reflection;
using System.Net;

namespace MailmanSharp.Tests
{
    public class MailmanClientTests
    {
        private readonly IMailmanClientInternal _client;
        private readonly Mock<IRestClient> _restClientMock;
        private IRestRequest _passedRequest = null;

        private static string _adminUrl = "http://example.com/admin.cgi/foo-list";
        private static string _password = "password";

        public MailmanClientTests()
        {
            _client = new MailmanClient(new MailmanList());
            _restClientMock = new Mock<IRestClient>();
            
            _restClientMock.SetupProperty(r => r.BaseUrl);

            var restField = typeof(MailmanClient).GetField("_client", BindingFlags.Instance | BindingFlags.NonPublic);
            restField.SetValue(_client, _restClientMock.Object);
        }

        [Fact]
        public void ShortAdminUrl_Exception()
        {
            Action act = () => _client.AdminUrl = "http://example.com/admin.cgi";
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void EmptyAdminUrl_Doesnt_Throw(string input)
        {
            _client.AdminUrl = _adminUrl;
            _client.AdminUrl = input;

            _client.AdminUrl.Should().Be("");
        }

        [Fact]
        public void AdminUrl_Sets_Other_Properties()
        {
            _client.AdminUrl = _adminUrl;

            var info = typeof(MailmanClient).GetField("_adminPath", BindingFlags.Instance | BindingFlags.NonPublic);
            info.GetValue(_client).Should().Be("admin.cgi");

            info = typeof(MailmanClient).GetField("_listName", BindingFlags.Instance | BindingFlags.NonPublic);
            info.GetValue(_client).Should().Be("foo-list");

            _restClientMock.VerifySet(r => r.BaseUrl = new Uri("http://example.com/"));
        }

        private void SetupCallback(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _restClientMock.Setup(r => r.ExecuteTaskAsync(It.IsAny<IRestRequest>()))
                .ReturnsAsync(new RestResponse() { StatusCode = statusCode })
                .Callback<IRestRequest>(rr => _passedRequest = rr)
                .Verifiable();
        }

        [Fact]
        public async Task ExecuteRosterRequest()
        {
            SetupCallback();

            var cookies = new CookieContainer();
            cookies.Add(new Uri("http://example.com/"), new Cookie("foo-list+admin", "foobar"));
            _restClientMock.Setup(r => r.CookieContainer).Returns(cookies);

            _client.AdminUrl = _adminUrl;
            _client.AdminPassword = _password;

            var expected = new RestRequest()
            {
                Method = Method.GET,
                Resource = "roster.cgi/foo-list",
            };
            expected.AddParameter("adminpw", _password);

            await _client.ExecuteRosterRequestAsync();

            _restClientMock.VerifyAll();
            _passedRequest.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task ExecuteGetAdminRequest()
        {
            SetupCallback();

            _client.AdminUrl = _adminUrl;
            _client.AdminPassword = _password;

            var expected = new RestRequest()
            {
                Method = Method.GET,
                Resource = "admin.cgi/foo-list/privacy",               
            };
            expected.AddParameter("adminpw", _password)
                .AddParameter("foo", "bar");

            await _client.ExecuteAdminRequestAsync(Method.GET, "privacy", ("foo", "bar"));

            _restClientMock.Verify();
            _passedRequest.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ExecuteGetAdminRequest_ShouldFail()
        {
            SetupCallback(HttpStatusCode.BadRequest);

            _client.AdminUrl = _adminUrl;
            _client.AdminPassword = _password;

            var expected = new RestRequest()
            {
                Method = Method.GET,
                Resource = "admin.cgi/foo-list/privacy",
            };
            expected.AddParameter("adminpw", _password)
                .AddParameter("foo", "bar");

            Func<Task> act = async () => await _client.ExecuteAdminRequestAsync(Method.GET, "privacy", ("foo", "bar"));
            act.Should().Throw<MailmanHttpException>();

            _restClientMock.Verify();
            _passedRequest.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task ExecutePostAdminRequest()
        {
            SetupCallback();

            _client.AdminUrl = _adminUrl;
            _client.AdminPassword = _password;

            var expected = new RestRequest()
            {
                Method = Method.POST,
                Resource = "admin.cgi/foo-list/privacy",
            };
            expected.AddParameter("adminpw", _password)
                .AddParameter("foo", "bar");

            await _client.ExecuteAdminRequestAsync(Method.POST, "privacy", ("foo", "bar"));

            _restClientMock.Verify();
            _passedRequest.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ExecutePostAdminRequest_ShouldFail()
        {
            SetupCallback(HttpStatusCode.BadRequest);

            _client.AdminUrl = _adminUrl;
            _client.AdminPassword = _password;

            var expected = new RestRequest()
            {
                Method = Method.POST,
                Resource = "admin.cgi/foo-list/privacy",
            };
            expected.AddParameter("adminpw", _password)
                .AddParameter("foo", "bar");

            Func<Task> act = async () => await _client.ExecuteAdminRequestAsync(Method.POST, "privacy", ("foo", "bar"));
            act.Should().Throw<MailmanHttpException>();

            _restClientMock.Verify();
            _passedRequest.Should().BeEquivalentTo(expected);
        }
    }
}
