using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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
            _saved.SubscribePolicy = Inc(_saved.SubscribePolicy.Value);
            _saved.SubscribeAutoApproval = GuidEmailArray(3);

            _saved.DefaultMemberModeration = !_saved.DefaultMemberModeration;
            _saved.MemberVerbosityThreshold = Inc(_saved.MemberVerbosityThreshold);
            _saved.MemberModerationNotice = Guid.NewGuid().ToString();
            _saved.DmarcModerationAction = Inc(_saved.DmarcModerationAction.Value);

            _saved.RequireExplicitDestination = !_saved.RequireExplicitDestination;
            _saved.AcceptableAliases = GuidEmailArray(4);
            _saved.MaxNumRecipients = Inc(_saved.MaxNumRecipients);

            //_saved.HeaderFilterRules.Should().NotBeNull()
            //    .And.HaveCountGreaterOrEqualTo(1);
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
    }
}
