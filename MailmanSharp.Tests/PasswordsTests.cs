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
using System.IO;

namespace MailmanSharp.Tests
{
    public class PasswordsTests
    {
        private readonly Mock<IMailmanClientInternal> _clientMock;
        private readonly IMailmanList _list;
        private IRestRequest _request;
        private IEnumerable<string> _parmStrings => _request.Parameters.Select(p => p.ToString());

        public PasswordsTests()
        {
            _list = new MailmanList();

            _clientMock = new Mock<IMailmanClientInternal>();
            _clientMock.Setup(c => c.ExecuteAdminRequestAsync(It.IsAny<string>(), It.IsAny<IRestRequest>()))
                 .ReturnsAsync(new RestResponse()
                 {
                     StatusCode = System.Net.HttpStatusCode.OK
                 })
                .Callback<string, IRestRequest>((i, r) => _request = r);
            _clientMock.Setup(c => c.ExecuteRosterRequestAsync())
               .ReturnsAsync(new RestResponse()
               {
                   Content = File.ReadAllText("roster.html"),
                   StatusCode = System.Net.HttpStatusCode.OK
               });
            _clientMock.Setup(c => c.Clone())
                .Returns(_clientMock.Object);


            var prop = typeof(MailmanList).GetProperty("InternalClient", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(_list, _clientMock.Object);
        }

        private void VerifyRequest(string slug, string value)
        {
            _parmStrings.Should().Contain($"new{slug}pw={value}");
            _parmStrings.Should().Contain($"confirm{slug}pw={value}");
        }

        [Fact]
        public async Task Admin_Should_Pass_Correct_Params()
        {
            var guid = Guid.NewGuid().ToString();
            _list.Passwords.Administrator = guid;
            await _list.Passwords.WriteAsync();
            VerifyRequest("", guid);
        }

        [Fact]
        public async Task Moderator_Should_Pass_Correct_Params()
        {
            var guid = Guid.NewGuid().ToString();
            _list.Passwords.Moderator = guid;
            await _list.Passwords.WriteAsync();
            VerifyRequest("mod", guid);
        }

        [Fact]
        public async Task Poster_Should_Pass_Correct_Params()
        {
            var guid = Guid.NewGuid().ToString();
            _list.Passwords.Poster = guid;
            await _list.Passwords.WriteAsync();
            VerifyRequest("post", guid);
        }
    }
}
