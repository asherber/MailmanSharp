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
    public class MembershipTests
    {
        private readonly Mock<IMailmanClientInternal> _clientMock;
        private readonly IMailmanList _list;
        private IRestRequest _request;
        private IEnumerable<string> _parmStrings => _request.Parameters.Select(p => p.ToString());

        public MembershipTests()
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Toggle_Moderate_Sends_Right_Params(bool value)
        {
            await _list.Membership.SetModerateAllAsync(value);

            _request.Should().NotBeNull();
            _parmStrings.Should().Contain("allmodbit_btn=1");
            _parmStrings.Should().Contain($"allmodbit_val={value.ToInt()}");
        }

        [Theory]
        [InlineData(UnsubscribeOptions.None)]
        [InlineData(UnsubscribeOptions.NotifyOwner)]
        [InlineData(UnsubscribeOptions.SendAcknowledgement)]
        [InlineData(UnsubscribeOptions.NotifyOwner | UnsubscribeOptions.SendAcknowledgement)]
        public async Task Unsubscribe_Sends_Right_Params(UnsubscribeOptions options)
        {
            await _list.Membership.UnsubscribeAsync("foo", options);

            var ack = options.HasFlag(UnsubscribeOptions.SendAcknowledgement);
            var owner = options.HasFlag(UnsubscribeOptions.NotifyOwner);

            _request.Should().NotBeNull();
            _parmStrings.Should().Contain("unsubscribees=foo");
            _parmStrings.Should().Contain($"send_unsub_ack_to_this_batch={ack.ToInt()}");
            _parmStrings.Should().Contain($"send_unsub_notifications_to_list_owner={owner.ToInt()}");
        }

        [Fact]
        public async Task Unsub_All_Works()
        {
            await _list.Membership.UnsubscribeAllAsync();

            _request.Should().NotBeNull();
            _parmStrings.Should().Contain("unsubscribees=aaron@sherber.com\naaron.sherber@sterlingts.com");
        }

        [Theory]
        [InlineData(null, SubscribeOptions.None)]
        [InlineData(null, SubscribeOptions.NotifyOwner)]
        [InlineData(null, SubscribeOptions.SendWelcomeMessage)]
        [InlineData(null, SubscribeOptions.NotifyOwner | SubscribeOptions.SendWelcomeMessage)]
        [InlineData("my message", SubscribeOptions.None)]
        [InlineData("my message", SubscribeOptions.NotifyOwner)]
        [InlineData("my message", SubscribeOptions.SendWelcomeMessage)]
        [InlineData("my message", SubscribeOptions.NotifyOwner | SubscribeOptions.SendWelcomeMessage)]
        public async Task Subscribe_Sends_Right_Params(string message, SubscribeOptions options)
        {
            await _list.Membership.SubscribeAsync("bob@example.com", message, options);

            var welcome = options.HasFlag(SubscribeOptions.SendWelcomeMessage);
            var owner = options.HasFlag(SubscribeOptions.NotifyOwner);

            _request.Should().NotBeNull();
            _parmStrings.Should().Contain("subscribees=bob@example.com");
            _parmStrings.Should().Contain("subscribe_or_invite=0");
            _parmStrings.Should().Contain($"send_welcome_msg_to_this_batch={welcome.ToInt()}");
            _parmStrings.Should().Contain($"send_notifications_to_list_owner={owner.ToInt()}");
            if (!String.IsNullOrEmpty(message))
                _parmStrings.Should().Contain($"invitation={message}\n\n");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("my message")]
        public async Task Invite_Sends_Right_Params(string message)
        {
            await _list.Membership.InviteAsync("bob@example.com", message);

            _request.Should().NotBeNull();
            _parmStrings.Should().Contain("subscribees=bob@example.com");
            _parmStrings.Should().Contain("subscribe_or_invite=1");
            if (!String.IsNullOrEmpty(message))
                _parmStrings.Should().Contain($"invitation={message}\n\n");
        }
    }
}
