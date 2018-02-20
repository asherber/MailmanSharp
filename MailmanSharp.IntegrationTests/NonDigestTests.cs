using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using FluentAssertions.Json;
using Xunit;

namespace MailmanSharp.IntegrationTests
{
    public class NonDigestTests : BaseTests
    {
        private static NonDigestSection _saved;
        private static string _config;

        private NonDigestSection Section => _list.NonDigest;

        public override async Task P10_ReadValues()
        {
            await Section.ReadAsync();

            Section.Nondigestable.Should().NotBeNull();
            Section.Personalize.Should().NotBeNull();
            Section.MsgHeader.Should().NotBeNull();
            Section.MsgFooter.Should().NotBeNull();
            Section.ScrubNondigest.Should().NotBeNull();
            Section.RegularExcludeLists.Should().NotBeEmpty()
                .And.HaveCountGreaterOrEqualTo(1);

            _saved = Section;
        }

        public override async Task P20_ChangeAndSave()
        {
            _saved.Nondigestable = !_saved.Nondigestable;
            _saved.Personalize = Inc(_saved.Personalize);
            _saved.MsgHeader = Guid.NewGuid().ToString();
            _saved.MsgFooter = Guid.NewGuid().ToString();
            _saved.ScrubNondigest = !_saved.ScrubNondigest;
            Section.RegularExcludeLists = GuidEmailArray(3);

            await _saved.WriteAsync();
        }

        public override async Task P30_ReadAndVerifyChanges()
        {
            await Section.ReadAsync();
            Section.Should().BeEquivalentTo(_saved);
        }

        public override async Task P40_LoadJsonAndSave()
        {
            _config = SampleConfig("NonDigest");
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
        public async Task RegularIncludeLists_Must_Be_Email()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.RegularIncludeLists.Add(guid);

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option regular_include_lists: {guid}");
        }

        [Fact]
        public async Task RegularExcludeLists_Must_Be_Email()
        {
            await Section.ReadAsync();
            var guid = Guid.NewGuid().ToString();
            Section.RegularExcludeLists.Add(guid);

            Func<Task> act = async () => await Section.WriteAsync();
            act.Should().Throw<MailmanException>().WithMessage($"Bad email address for option regular_exclude_lists: {guid}");
        }
    }
}
