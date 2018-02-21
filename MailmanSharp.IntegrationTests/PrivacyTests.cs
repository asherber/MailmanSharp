using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions.Json;

namespace MailmanSharp.IntegrationTests
{
    public class PrivacyTests : BaseTests
    {
        private static PrivacySection _saved;
        private static string _config;

        private PrivacySection Section => _list.Privacy;

        public override async Task P10_ReadValues()
        {
            await Section.ReadAsync();

            Section.Advertised.Should().NotBeNull();
            Section.SubscribePolicy.Should().NotBeNull();
            Section.SubscribeAutoApproval.Should().NotBeNull()
                .And.HaveCountGreaterOrEqualTo(1);

            Section.DefaultMemberModeration.Should().NotBeNull();
            Section.MemberVerbosityThreshold.Should().NotBeNull();
            Section.MemberModerationNotice.Should().NotBeNull();
            Section.DmarcModerationAction.Should().NotBeNull();

            Section.RequireExplicitDestination.Should().NotBeNull();
            Section.AcceptableAliases.Should().NotBeNull()
                .And.HaveCountGreaterOrEqualTo(1);
            Section.MaxNumRecipients.Should().NotBeNull();

            Section.HeaderFilterRules.Should().NotBeNull()
                .And.HaveCountGreaterOrEqualTo(1);
            Section.BounceMatchingHeaders.Should().NotBeNull()
                .And.HaveCountGreaterOrEqualTo(1);

            _saved = Section;
        }

        public override async Task P20_ChangeAndSave()
        {
            _saved.Advertised = !_saved.Advertised;
            _saved.SubscribePolicy = Inc(_saved.SubscribePolicy);
            _saved.SubscribeAutoApproval = GuidEmailArray(3);

            _saved.DefaultMemberModeration = !_saved.DefaultMemberModeration;
            _saved.MemberVerbosityThreshold = Inc(_saved.MemberVerbosityThreshold);
            _saved.MemberModerationNotice = Guid.NewGuid().ToString();
            _saved.DmarcModerationAction = Inc(_saved.DmarcModerationAction);

            _saved.RequireExplicitDestination = !_saved.RequireExplicitDestination;
            _saved.AcceptableAliases = GuidEmailArray(4);
            _saved.MaxNumRecipients = Inc(_saved.MaxNumRecipients);

            _saved.HeaderFilterRules = new List<HeaderFilterRule>() { new HeaderFilterRule()
            {
                Action = HeaderFilterAction.Accept,
                Regexes = new List<string>() { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }
            } };
            _saved.BounceMatchingHeaders = GuidEmailArray(3);

            await _saved.WriteAsync();
        }

        public override async Task P30_ReadAndVerifyChanges()
        {
            await Section.ReadAsync();
            Section.Should().BeEquivalentTo(_saved);
        }

        public override async Task P40_LoadJsonAndSave()
        {
            _config = SampleConfig("Privacy");
            Section.LoadConfig(_config);
            await Section.WriteAsync();
        }

        public override async Task P50_ReadAndVerifyJson()
        {
            var expected = JObject.Parse(_config);

            await Section.ReadAsync();
            var output = JObject.Parse(Section.CurrentConfig);
            output.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task SubscribeAutoApproval_Must_Be_Email()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.SubscribeAutoApproval.Add(guid);

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option subscribe_auto_approval: {guid}");
        }

        [Fact]
        public async Task BanList_Must_Be_Email()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.BanList.Add(guid);

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option ban_list: {guid}");
        }

        [Fact]
        public async Task AcceptTheseNonmembers_Must_Be_Email()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.AcceptTheseNonmembers.Add(guid);

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option accept_these_nonmembers: {guid}");
        }
        [Fact]
        public async Task HoldTheseNonmembers_Must_Be_Email()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.HoldTheseNonmembers.Add(guid);

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option hold_these_nonmembers: {guid}");
        }

        [Fact]
        public async Task RejectTheseNonmembers_Must_Be_Email()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.RejectTheseNonmembers.Add(guid);

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option reject_these_nonmembers: {guid}");
        }

        [Fact]
        public async Task DiscardTheseNonmembers_Must_Be_Email()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.DiscardTheseNonmembers.Add(guid);

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option discard_these_nonmembers: {guid}");
        }
    }
}
